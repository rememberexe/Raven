namespace RavenMobile.Features.Transfer.Model;

public class TransferProgress
{
    public string CurrentFileName { get; set; } = "";
    public int CurrentFileIndex { get; set; }
    public int TotalFiles { get; set; }

    public long SentBytes { get; set; }
    public long TotalBytes { get; set; }

    public double Percent => TotalBytes <= 0 ? 0 : SentBytes * 100.0 / TotalBytes;

    public string SpeedText { get; set; } = "0 MB/s";
    public string RemainingTimeText { get; set; } = "";
}