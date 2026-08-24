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

namespace RavenMobile.Features.Transfer;

public class TcpTransferService : ITransferService
{
    private const int Port = 50555;
    private const int BufferSize = 128 * 1024;

    private const string ProtocolRequest = "RAVEN_TRANSFER_REQUEST_V1";
    private const string ProtocolAccept = "RAVEN_ACCEPT";
    private const string ProtocolReject = "RAVEN_REJECT";
    private const string ProtocolDone = "RAVEN_DONE";

    private bool _isServerRunning;
    private CancellationTokenSource? _sessionCts;

    public event Action<string>? OnTransferStatusChanged;
    public event Action<TransferProgress>? OnTransferProgressChanged;
    public event Func<IncomingTransferRequest, Task<bool>>? OnIncomingTransferRequest;
    public event Action? OnTransferSessionFinished;

    public Task StartGroupOwnerSessionAsync(IReadOnlyList<SelectedFileItem> pendingFiles)
    {
        if (_isServerRunning)
        {
            OnTransferStatusChanged?.Invoke("TCP sunucu zaten çalışıyor.");
            return Task.CompletedTask;
        }

        var filesSnapshot = pendingFiles.ToList();

        _sessionCts = new CancellationTokenSource();
        _isServerRunning = true;

        _ = Task.Run(async () =>
        {
            await RunGroupOwnerServerOnceAsync(filesSnapshot, _sessionCts.Token);
        });

        return Task.CompletedTask;
    }

    public async Task ConnectToGroupOwnerSessionAsync(string groupOwnerAddress, IReadOnlyList<SelectedFileItem> pendingFiles)
    {
        try
        {
            var filesSnapshot = pendingFiles.ToList();

            OnTransferStatusChanged?.Invoke("Group Owner cihazına bağlanılıyor...");

            using var client = await ConnectWithRetryAsync(groupOwnerAddress, Port, TimeSpan.FromSeconds(15));
            using var stream = client.GetStream();

            await RunTransferSessionAsync(stream, filesSnapshot, CancellationToken.None);
        }
        catch (Exception ex)
        {
            OnTransferStatusChanged?.Invoke($"Bağlantı hatası: {ex.Message}");
        }
        finally
        {
            OnTransferSessionFinished?.Invoke();
        }
    }

    public Task StopSessionAsync()
    {
        try
        {
            _sessionCts?.Cancel();
        }
        catch
        {
        }

        return Task.CompletedTask;
    }

    private async Task RunGroupOwnerServerOnceAsync(
        IReadOnlyList<SelectedFileItem> pendingFiles,
        CancellationToken token)
    {
        TcpListener? listener = null;

        try
        {
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Start();

            if (pendingFiles.Count > 0)
                OnTransferStatusChanged?.Invoke("Bu cihaz Group Owner oldu. Karşı cihazın bağlanması bekleniyor...");
            else
                OnTransferStatusChanged?.Invoke("Alıcı hazır. Karşı cihaz bekleniyor...");

            using var client = await listener.AcceptTcpClientAsync(token);
            using var stream = client.GetStream();

            await RunTransferSessionAsync(stream, pendingFiles, token);
        }
        catch (OperationCanceledException)
        {
            OnTransferStatusChanged?.Invoke("Transfer oturumu iptal edildi.");
        }
        catch (Exception ex)
        {
            OnTransferStatusChanged?.Invoke($"Sunucu bağlantı hatası: {ex.Message}");
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

            _isServerRunning = false;
            OnTransferSessionFinished?.Invoke();
        }
    }

    private async Task RunTransferSessionAsync(
        NetworkStream stream,
        IReadOnlyList<SelectedFileItem> pendingFiles,
        CancellationToken token)
    {
        if (pendingFiles.Count > 0)
        {
            await SendOutgoingTransferAsync(stream, pendingFiles, token);
        }
        else
        {
            await ReceiveIncomingTransferAsync(stream, token);
        }
    }

    private async Task SendOutgoingTransferAsync(
        NetworkStream stream,
        IReadOnlyList<SelectedFileItem> files,
        CancellationToken token)
    {
        var validFiles = files
            .Where(f => !string.IsNullOrWhiteSpace(f.FullPath) && File.Exists(f.FullPath))
            .ToList();

        if (validFiles.Count == 0)
        {
            OnTransferStatusChanged?.Invoke("Geçerli dosya bulunamadı.");
            return;
        }

        var senderName = DeviceInfo.Current.Name;

        if (string.IsNullOrWhiteSpace(senderName))
            senderName = "Raven cihazı";

        var totalBytes = validFiles.Sum(f => f.Size);

        await WriteStringAsync(stream, ProtocolRequest, token);
        await WriteStringAsync(stream, senderName, token);
        await WriteInt32Async(stream, validFiles.Count, token);
        await WriteInt64Async(stream, totalBytes, token);

        foreach (var file in validFiles)
        {
            await WriteStringAsync(stream, file.FileName, token);
            await WriteInt64Async(stream, file.Size, token);
        }

        OnTransferStatusChanged?.Invoke("Alıcı onayı bekleniyor...");

        var response = await ReadStringAsync(stream, token);

        if (response == ProtocolReject)
        {
            OnTransferStatusChanged?.Invoke("Alıcı dosya aktarımını reddetti.");
            return;
        }

        if (response != ProtocolAccept)
        {
            OnTransferStatusChanged?.Invoke("Alıcı geçersiz yanıt verdi.");
            return;
        }

        OnTransferStatusChanged?.Invoke("Alıcı kabul etti. Dosyalar gönderiliyor...");

        long sentTotal = 0;
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < validFiles.Count; i++)
        {
            var file = validFiles[i];

            using var fileStream = File.OpenRead(file.FullPath);

            var buffer = new byte[BufferSize];
            int read;

            while ((read = await fileStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                await stream.WriteAsync(buffer.AsMemory(0, read), token);

                sentTotal += read;

                var elapsed = Math.Max(stopwatch.Elapsed.TotalSeconds, 0.1);
                var speed = sentTotal / elapsed;
                var remainingBytes = Math.Max(totalBytes - sentTotal, 0);
                var remainingSeconds = speed <= 0 ? 0 : remainingBytes / speed;

                OnTransferProgressChanged?.Invoke(new TransferProgress
                {
                    CurrentFileName = file.FileName,
                    CurrentFileIndex = i + 1,
                    TotalFiles = validFiles.Count,
                    SentBytes = sentTotal,
                    TotalBytes = totalBytes,
                    SpeedText = FormatSpeed(speed),
                    RemainingTimeText = FormatTime(remainingSeconds)
                });
            }
        }

        OnTransferStatusChanged?.Invoke("Alıcının kaydetmesi bekleniyor...");

        var finalResponse = await ReadStringAsync(stream, token);

        if (finalResponse == ProtocolDone)
            OnTransferStatusChanged?.Invoke("Gönderim tamamlandı. Alıcı dosyayı kaydetti.");
        else
            OnTransferStatusChanged?.Invoke("Dosya gönderildi ama alıcıdan tamamlandı onayı gelmedi.");
    }

    private async Task ReceiveIncomingTransferAsync(NetworkStream stream, CancellationToken token)
    {
        var protocol = await ReadStringAsync(stream, token);

        if (protocol != ProtocolRequest)
        {
            OnTransferStatusChanged?.Invoke("Geçersiz Raven isteği.");
            return;
        }

        var senderName = await ReadStringAsync(stream, token);
        var fileCount = await ReadInt32Async(stream, token);
        var totalBytes = await ReadInt64Async(stream, token);

        var incomingFiles = new List<IncomingFileInfo>();

        for (int i = 0; i < fileCount; i++)
        {
            var fileName = await ReadStringAsync(stream, token);
            var fileSize = await ReadInt64Async(stream, token);

            incomingFiles.Add(new IncomingFileInfo
            {
                FileName = fileName,
                Size = fileSize
            });
        }

        var request = new IncomingTransferRequest
        {
            SenderName = senderName,
            FileCount = fileCount,
            TotalBytes = totalBytes,
            FileNames = incomingFiles.Select(f => f.FileName).ToList()
        };

        OnTransferStatusChanged?.Invoke($"{senderName} dosya göndermek istiyor.");

        var accepted = await AskUserForPermissionAsync(request);

        if (!accepted)
        {
            await WriteStringAsync(stream, ProtocolReject, token);
            OnTransferStatusChanged?.Invoke("Dosya aktarımı reddedildi.");
            return;
        }

        await WriteStringAsync(stream, ProtocolAccept, token);

        OnTransferStatusChanged?.Invoke($"{fileCount} dosya alınıyor...");

        var tempRoot = Path.Combine(FileSystem.CacheDirectory, "RavenReceivedTemp");
        Directory.CreateDirectory(tempRoot);

        long receivedTotal = 0;

        for (int i = 0; i < incomingFiles.Count; i++)
        {
            var incomingFile = incomingFiles[i];

            var safeName = MakeSafeFileName(incomingFile.FileName);
            var tempPath = GetUniquePath(Path.Combine(tempRoot, safeName));

            var received = await ReceiveFileAsync(
                stream,
                tempPath,
                incomingFile.FileName,
                incomingFile.Size,
                i + 1,
                incomingFiles.Count,
                receivedTotal,
                totalBytes,
                token);

            receivedTotal += received;

            await SaveReceivedFileToPublicAsync(tempPath, incomingFile.FileName);

            try
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch
            {
            }
        }

        await WriteStringAsync(stream, ProtocolDone, token);

        OnTransferStatusChanged?.Invoke("Dosya alımı tamamlandı.");
    }

    private async Task<TcpClient> ConnectWithRetryAsync(string hostAddress, int port, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        var attempt = 1;
        Exception? lastError = null;

        while (DateTime.UtcNow - start < timeout)
        {
            TcpClient? client = null;

            try
            {
                client = new TcpClient();

                var connectTask = client.ConnectAsync(hostAddress, port);
                var timeoutTask = Task.Delay(1500);

                var completed = await Task.WhenAny(connectTask, timeoutTask);

                if (completed == connectTask)
                {
                    await connectTask;

                    OnTransferStatusChanged?.Invoke("TCP bağlantısı kuruldu.");
                    return client;
                }

                client.Dispose();
                lastError = new TimeoutException("TCP bağlantı zaman aşımına uğradı.");
            }
            catch (Exception ex)
            {
                client?.Dispose();
                lastError = ex;
            }

            OnTransferStatusChanged?.Invoke($"Karşı cihaz hazırlanıyor... Deneme {attempt}");
            attempt++;

            await Task.Delay(500);
        }

        throw new IOException("Karşı cihaz TCP için hazır olmadı.", lastError);
    }

    private async Task<bool> AskUserForPermissionAsync(IncomingTransferRequest request)
    {
        var handler = OnIncomingTransferRequest;

        if (handler == null)
            return false;

        foreach (Func<IncomingTransferRequest, Task<bool>> callback in handler.GetInvocationList())
            return await callback(request);

        return false;
    }

    private async Task<long> ReceiveFileAsync(
        NetworkStream stream,
        string tempPath,
        string fileName,
        long fileSize,
        int fileIndex,
        int totalFiles,
        long receivedTotalBeforeThisFile,
        long totalBytes,
        CancellationToken token)
    {
        var buffer = new byte[BufferSize];
        long receivedFileBytes = 0;

        var stopwatch = Stopwatch.StartNew();

        using var fileStream = File.Create(tempPath);

        while (receivedFileBytes < fileSize)
        {
            var remaining = fileSize - receivedFileBytes;
            var readSize = (int)Math.Min(buffer.Length, remaining);

            var read = await stream.ReadAsync(buffer, 0, readSize, token);

            if (read <= 0)
                throw new IOException("Bağlantı kesildi.");

            await fileStream.WriteAsync(buffer.AsMemory(0, read), token);

            receivedFileBytes += read;

            var totalReceivedNow = receivedTotalBeforeThisFile + receivedFileBytes;

            var elapsed = Math.Max(stopwatch.Elapsed.TotalSeconds, 0.1);
            var speed = receivedFileBytes / elapsed;
            var remainingBytes = Math.Max(totalBytes - totalReceivedNow, 0);
            var remainingSeconds = speed <= 0 ? 0 : remainingBytes / speed;

            OnTransferProgressChanged?.Invoke(new TransferProgress
            {
                CurrentFileName = fileName,
                CurrentFileIndex = fileIndex,
                TotalFiles = totalFiles,
                SentBytes = totalReceivedNow,
                TotalBytes = totalBytes,
                SpeedText = FormatSpeed(speed),
                RemainingTimeText = FormatTime(remainingSeconds)
            });
        }

        return receivedFileBytes;
    }

    private async Task SaveReceivedFileToPublicAsync(string tempPath, string fileName)
    {
#if ANDROID
        var context = AppContext.Context;
        var resolver = context.ContentResolver;

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

            OnTransferStatusChanged?.Invoke($"Kaydedildi: {relativePath}/{safeName}");
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
#else
        var saveRoot = Path.Combine(FileSystem.AppDataDirectory, "RavenReceived");
        Directory.CreateDirectory(saveRoot);

        var savePath = GetUniquePath(Path.Combine(saveRoot, MakeSafeFileName(fileName)));
        File.Copy(tempPath, savePath, true);

        OnTransferStatusChanged?.Invoke($"Kaydedildi: {savePath}");
#endif
    }

    private static async Task WriteInt32Async(Stream stream, int value, CancellationToken token)
    {
        var data = BitConverter.GetBytes(value);
        await stream.WriteAsync(data.AsMemory(0, data.Length), token);
    }

    private static async Task<int> ReadInt32Async(Stream stream, CancellationToken token)
    {
        var data = new byte[4];
        await stream.ReadExactlyAsync(data, token);
        return BitConverter.ToInt32(data);
    }

    private static async Task WriteInt64Async(Stream stream, long value, CancellationToken token)
    {
        var data = BitConverter.GetBytes(value);
        await stream.WriteAsync(data.AsMemory(0, data.Length), token);
    }

    private static async Task<long> ReadInt64Async(Stream stream, CancellationToken token)
    {
        var data = new byte[8];
        await stream.ReadExactlyAsync(data, token);
        return BitConverter.ToInt64(data);
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
        await stream.ReadExactlyAsync(data, token);
        return Encoding.UTF8.GetString(data);
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