namespace RavenMobile.Features.Transfer.Model;

public class IncomingTransferRequest
{
    public string SenderName { get; set; } = "Raven cihazı";

    public int FileCount { get; set; }

    public long TotalBytes { get; set; }

    public List<string> FileNames { get; set; } = new();

    public string TotalSizeText
    {
        get
        {
            if (TotalBytes < 1024)
                return $"{TotalBytes} B";

            if (TotalBytes < 1024 * 1024)
                return $"{TotalBytes / 1024.0:F1} KB";

            if (TotalBytes < 1024 * 1024 * 1024)
                return $"{TotalBytes / 1024.0 / 1024.0:F1} MB";

            return $"{TotalBytes / 1024.0 / 1024.0 / 1024.0:F1} GB";
        }
    }
}