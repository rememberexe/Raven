using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RavenMobile.Features.Transfer.Model;

#if ANDROID
using Android.Content;
using Android.Provider;
using Android.Webkit;
using AndroidEnvironment = Android.OS.Environment;
using AppContext = Android.App.Application;
#endif

namespace RavenMobile.Features.WifiQr;

public class WifiQrTransferService : IWifiQrTransferService
{
    private TcpListener? _listener;
    private const int Port = 50555;
    private const int BufferSize = 128 * 1024;
    private const string Protocol = "RAVEN_QR_TRANSFER_V1";

    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event Action<string>? OnStatusChanged;
    public event Action<TransferProgress>? OnProgressChanged;

    public Task StartReceiverAsync()
    {
        if (_isRunning)
        {
            OnStatusChanged?.Invoke("Alıcı zaten dosya bekliyor.");
            return Task.CompletedTask;
        }

        _cts = new CancellationTokenSource();
        _isRunning = true;

        _ = Task.Run(() => ReceiverLoopAsync(_cts.Token));

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        try
        {
            _cts?.Cancel();
            _cts = null;

            try
            {
                _listener?.Stop();
            }
            catch
            {
            }

            _listener = null;
            _isRunning = false;

            OnStatusChanged?.Invoke("Dosya alma durduruldu.");
        }
        catch
        {
        }

        return Task.CompletedTask;
    }

    private async Task ReceiverLoopAsync(CancellationToken token)
    {
        TcpListener? listener = null;

        try
        {
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Start();

            _listener = listener;

            OnStatusChanged?.Invoke("Dosya bekleniyor...");

            while (!token.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(token);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleClientAsync(client, token);
                    }
                    catch (Exception ex)
                    {
                        OnStatusChanged?.Invoke($"Alma hatası: {ex.Message}");
                    }
                }, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            OnStatusChanged?.Invoke($"Alıcı sunucu hatası: {ex.Message}");
        }
        finally
        {
            try
            {
                listener?.Stop();
            }
            catch
            {
            }

            _listener = null;
            _isRunning = false;
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        using (var stream = client.GetStream())
        {
            var protocol = await ReadStringAsync(stream, token);

            if (protocol != Protocol)
            {
                OnStatusChanged?.Invoke("Geçersiz Raven QR isteği.");
                return;
            }

            var senderName = await ReadStringAsync(stream, token);
            var fileCount = await ReadInt32Async(stream, token);
            var totalBytes = await ReadInt64Async(stream, token);

            OnStatusChanged?.Invoke($"{senderName} cihazından {fileCount} dosya alınıyor...");

            var files = new List<IncomingFileInfo>();

            for (int i = 0; i < fileCount; i++)
            {
                files.Add(new IncomingFileInfo
                {
                    FileName = await ReadStringAsync(stream, token),
                    Size = await ReadInt64Async(stream, token)
                });
            }

            var tempRoot = Path.Combine(FileSystem.CacheDirectory, "RavenQrTemp");
            Directory.CreateDirectory(tempRoot);

            long receivedTotal = 0;
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                var safeName = MakeSafeFileName(file.FileName);
                var tempPath = GetUniquePath(Path.Combine(tempRoot, safeName));

                long fileReceived = 0;
                var buffer = new byte[BufferSize];

                // ÖNEMLİ:
                // Dosyayı önce tamamen yazıyoruz.
                // Sonra FileStream kapanıyor.
                // Sonra MediaStore'a kaydediyoruz.
                await using (var fileStream = new FileStream(
                    tempPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read,
                    BufferSize,
                    useAsync: true))
                {
                    while (fileReceived < file.Size)
                    {
                        var remaining = file.Size - fileReceived;
                        var readSize = (int)Math.Min(BufferSize, remaining);

                        var read = await stream.ReadAsync(buffer.AsMemory(0, readSize), token);

                        if (read <= 0)
                            throw new IOException("Bağlantı kesildi.");

                        await fileStream.WriteAsync(buffer.AsMemory(0, read), token);

                        fileReceived += read;
                        receivedTotal += read;

                        var elapsed = Math.Max(stopwatch.Elapsed.TotalSeconds, 0.1);
                        var speed = receivedTotal / elapsed;
                        var remainingBytes = Math.Max(totalBytes - receivedTotal, 0);
                        var remainingSeconds = speed <= 0 ? 0 : remainingBytes / speed;

                        OnProgressChanged?.Invoke(new TransferProgress
                        {
                            CurrentFileName = file.FileName,
                            CurrentFileIndex = i + 1,
                            TotalFiles = files.Count,
                            SentBytes = receivedTotal,
                            TotalBytes = totalBytes,
                            SpeedText = FormatSpeed(speed),
                            RemainingTimeText = FormatTime(remainingSeconds)
                        });
                    }

                    await fileStream.FlushAsync(token);
                }

                // FileStream artık kapandı. Şimdi güvenli şekilde kaydedebiliriz.
                await SaveReceivedFileToPublicAsync(tempPath, file.FileName);

                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                }
            }

            await WriteStringAsync(stream, "RAVEN_DONE", token);

            OnStatusChanged?.Invoke("Dosya alımı tamamlandı.");
        }
    }

    private async Task SaveReceivedFileToPublicAsync(string tempPath, string fileName)
    {
#if ANDROID
#pragma warning disable CA1416

        var context = AppContext.Context;

        if (context == null)
            throw new IOException("Android context alınamadı.");

        var resolver = context.ContentResolver;

        if (resolver == null)
            throw new IOException("ContentResolver alınamadı.");

        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        var mimeType = MimeTypeMap.Singleton?.GetMimeTypeFromExtension(extension);

        if (string.IsNullOrWhiteSpace(mimeType))
            mimeType = "application/octet-stream";

        var safeName = MakeSafeFileName(fileName);

        Android.Net.Uri collectionUri;
        string relativePath;

        if (mimeType.StartsWith("image/"))
        {
            collectionUri = MediaStore.Images.Media.ExternalContentUri!;
            relativePath = $"{AndroidEnvironment.DirectoryPictures}/Raven";
        }
        else if (mimeType.StartsWith("video/"))
        {
            collectionUri = MediaStore.Video.Media.ExternalContentUri!;
            relativePath = $"{AndroidEnvironment.DirectoryMovies}/Raven";
        }
        else if (mimeType.StartsWith("audio/"))
        {
            collectionUri = MediaStore.Audio.Media.ExternalContentUri!;
            relativePath = $"{AndroidEnvironment.DirectoryMusic}/Raven";
        }
        else
        {
            collectionUri = MediaStore.Downloads.ExternalContentUri!;
            relativePath = $"{AndroidEnvironment.DirectoryDownloads}/Raven";
        }

        var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, safeName);
        values.Put(MediaStore.IMediaColumns.MimeType, mimeType);
        values.Put(MediaStore.IMediaColumns.RelativePath, relativePath);
        values.Put(MediaStore.IMediaColumns.IsPending, 1);

        var itemUri = resolver.Insert(collectionUri, values);

        if (itemUri == null)
            throw new IOException("Dosya kaydı için MediaStore URI oluşturulamadı.");

        try
        {
            await using (var input = File.OpenRead(tempPath))
            await using (var output = resolver.OpenOutputStream(itemUri))
            {
                if (output == null)
                    throw new IOException("Dosya çıkış akışı açılamadı.");

                await input.CopyToAsync(output);
            }

            values.Clear();
            values.Put(MediaStore.IMediaColumns.IsPending, 0);
            resolver.Update(itemUri, values, null, null);

            OnStatusChanged?.Invoke($"Kaydedildi: {relativePath}/{safeName}");
        }
        catch
        {
            try
            {
                resolver.Delete(itemUri, null, null);
            }
            catch
            {
            }

            throw;
        }

#pragma warning restore CA1416
#else
        var saveRoot = Path.Combine(FileSystem.AppDataDirectory, "RavenReceived");
        Directory.CreateDirectory(saveRoot);

        var savePath = GetUniquePath(Path.Combine(saveRoot, MakeSafeFileName(fileName)));
        File.Copy(tempPath, savePath, true);

        OnStatusChanged?.Invoke($"Kaydedildi: {savePath}");
#endif
    }

    private static async Task WriteStringAsync(Stream stream, string value, CancellationToken token)
    {
        var data = Encoding.UTF8.GetBytes(value);
        await WriteInt32Async(stream, data.Length, token);
        await stream.WriteAsync(data.AsMemory(0, data.Length), token);
    }

    private static async Task<string> ReadStringAsync(Stream stream, CancellationToken token)
    {
        var length = await ReadInt32Async(stream, token);
        var data = new byte[length];
        await ReadExactlyInternalAsync(stream, data, token);
        return Encoding.UTF8.GetString(data);
    }

    private static async Task WriteInt32Async(Stream stream, int value, CancellationToken token)
    {
        var data = BitConverter.GetBytes(value);
        await stream.WriteAsync(data.AsMemory(0, data.Length), token);
    }

    private static async Task<int> ReadInt32Async(Stream stream, CancellationToken token)
    {
        var data = new byte[4];
        await ReadExactlyInternalAsync(stream, data, token);
        return BitConverter.ToInt32(data);
    }

    private static async Task ReadExactlyInternalAsync(Stream stream, byte[] buffer, CancellationToken token)
    {
        var offset = 0;

        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), token);

            if (read <= 0)
                throw new IOException("Bağlantı kesildi.");

            offset += read;
        }
    }

    private static async Task WriteInt64Async(Stream stream, long value, CancellationToken token)
    {
        var data = BitConverter.GetBytes(value);
        await stream.WriteAsync(data.AsMemory(0, data.Length), token);
    }

    private static async Task<long> ReadInt64Async(Stream stream, CancellationToken token)
    {
        var data = new byte[8];
        await ReadExactlyInternalAsync(stream, data, token);
        return BitConverter.ToInt64(data);
    }

    private static string FormatSpeed(double bytesPerSecond)
    {
        var mb = bytesPerSecond / 1024 / 1024;
        return $"{mb:F1} MB/s";
    }

    private static string FormatTime(double seconds)
    {
        if (seconds <= 1)
            return "az kaldı";

        var time = TimeSpan.FromSeconds(seconds);

        if (time.TotalMinutes < 1)
            return $"{time.Seconds} sn";

        return $"{(int)time.TotalMinutes} dk {time.Seconds} sn";
    }

    private static string MakeSafeFileName(string fileName)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c, '_');

        return fileName;
    }

    private static string GetUniquePath(string path)
    {
        if (!File.Exists(path))
            return path;

        var directory = Path.GetDirectoryName(path)!;
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);

        var index = 1;

        while (true)
        {
            var newPath = Path.Combine(directory, $"{name} ({index}){ext}");

            if (!File.Exists(newPath))
                return newPath;

            index++;
        }
    }

    private class IncomingFileInfo
    {
        public string FileName { get; set; } = "";
        public long Size { get; set; }
    }
}