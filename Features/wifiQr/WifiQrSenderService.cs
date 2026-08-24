using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using RavenMobile.Features.Transfer.Model;

namespace RavenMobile.Features.WifiQr;

public class WifiQrSenderService : IWifiQrSenderService
{
    private const int BufferSize = 128 * 1024;
    private const string Protocol = "RAVEN_QR_TRANSFER_V1";

    public event Action<string>? OnStatusChanged;
    public event Action<TransferProgress>? OnProgressChanged;

    public async Task SendFilesAsync(
        string hostAddress,
        int port,
        IReadOnlyList<SelectedFileItem> files)
    {
        try
        {
            var validFiles = files
                .Where(f => !string.IsNullOrWhiteSpace(f.FullPath) && File.Exists(f.FullPath))
                .ToList();

            if (validFiles.Count == 0)
            {
                OnStatusChanged?.Invoke("Geçerli dosya bulunamadı.");
                return;
            }

            OnStatusChanged?.Invoke($"Alıcıya bağlanılıyor: {hostAddress}:{port}");

            using var client = await ConnectWithRetryAsync(hostAddress, port, TimeSpan.FromSeconds(20));
            using var stream = client.GetStream();

            var senderName = DeviceInfo.Current.Name;

            if (string.IsNullOrWhiteSpace(senderName))
                senderName = "Raven cihazı";

            var totalBytes = validFiles.Sum(f => f.Size);

            await WriteStringAsync(stream, Protocol);
            await WriteStringAsync(stream, senderName);
            await WriteInt32Async(stream, validFiles.Count);
            await WriteInt64Async(stream, totalBytes);

            foreach (var file in validFiles)
            {
                await WriteStringAsync(stream, file.FileName);
                await WriteInt64Async(stream, file.Size);
            }

            OnStatusChanged?.Invoke("Dosyalar gönderiliyor...");

            long sentTotal = 0;
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < validFiles.Count; i++)
            {
                var file = validFiles[i];

                using var fileStream = File.OpenRead(file.FullPath);

                var buffer = new byte[BufferSize];
                int read;

                while ((read = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer.AsMemory(0, read));

                    sentTotal += read;

                    var elapsed = Math.Max(stopwatch.Elapsed.TotalSeconds, 0.1);
                    var speed = sentTotal / elapsed;
                    var remainingBytes = Math.Max(totalBytes - sentTotal, 0);
                    var remainingSeconds = speed <= 0 ? 0 : remainingBytes / speed;

                    OnProgressChanged?.Invoke(new TransferProgress
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

            OnStatusChanged?.Invoke("Alıcının kaydetmesi bekleniyor...");

            var response = await ReadStringAsync(stream);

            if (response == "RAVEN_DONE")
                OnStatusChanged?.Invoke("Gönderim tamamlandı. Alıcı dosyaları kaydetti.");
            else
                OnStatusChanged?.Invoke("Dosya gönderildi ama alıcıdan tamamlandı onayı gelmedi.");
        }
        catch (Exception ex)
        {
            OnStatusChanged?.Invoke($"Gönderim hatası: {ex.Message}");
        }
    }

    private async Task<TcpClient> ConnectWithRetryAsync(
        string hostAddress,
        int port,
        TimeSpan timeout)
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
                var timeoutTask = Task.Delay(2000);

                var completed = await Task.WhenAny(connectTask, timeoutTask);

                if (completed == connectTask)
                {
                    await connectTask;

                    OnStatusChanged?.Invoke("TCP bağlantısı kuruldu.");
                    return client;
                }

                client.Dispose();
                lastError = new TimeoutException("TCP bağlantı zaman aşımı.");
            }
            catch (Exception ex)
            {
                client?.Dispose();
                lastError = ex;
            }

            OnStatusChanged?.Invoke($"Alıcı hazırlanıyor... Deneme {attempt}");
            attempt++;

            await Task.Delay(700);
        }

        throw new IOException("Alıcı cihaza bağlanılamadı.", lastError);
    }

    private static async Task WriteStringAsync(Stream stream, string value)
    {
        var data = Encoding.UTF8.GetBytes(value);
        await WriteInt32Async(stream, data.Length);
        await stream.WriteAsync(data.AsMemory(0, data.Length));
    }

    private static async Task<string> ReadStringAsync(Stream stream)
    {
        var length = await ReadInt32Async(stream);
        var data = new byte[length];
        await ReadExactlyInternalAsync(stream, data);
        return Encoding.UTF8.GetString(data);
    }

    private static async Task WriteInt32Async(Stream stream, int value)
    {
        var data = BitConverter.GetBytes(value);
        await stream.WriteAsync(data.AsMemory(0, data.Length));
    }

    private static async Task<int> ReadInt32Async(Stream stream)
    {
        var data = new byte[4];
        await ReadExactlyInternalAsync(stream, data);
        return BitConverter.ToInt32(data);
    }

    private static async Task WriteInt64Async(Stream stream, long value)
    {
        var data = BitConverter.GetBytes(value);
        await stream.WriteAsync(data.AsMemory(0, data.Length));
    }

    private static async Task ReadExactlyInternalAsync(Stream stream, byte[] buffer)
    {
        var offset = 0;

        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset));

            if (read <= 0)
                throw new IOException("Bağlantı kesildi.");

            offset += read;
        }
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
}