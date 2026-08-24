using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;

namespace RavenMobile.Models;

public class TransferHistoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string FileName { get; set; } = "";
    public int FileCount { get; set; }
    public long TotalBytes { get; set; }

    // send / receive
    public string Direction { get; set; } = "send";

    public bool IsSuccess { get; set; } = true;

    public string Message { get; set; } = "";

    public string DeviceName { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public string FullDateText => CreatedAt.ToString("dd.MM.yyyy HH:mm");

    [JsonIgnore]
    public string DirectionText => Direction == "receive" ? "Alındı" : "Gönderildi";

    [JsonIgnore]
    public string Icon => Direction == "receive" ? "📥" : "📤";

    [JsonIgnore]
    public string StatusText => IsSuccess ? "Başarılı" : "Hatalı";

    [JsonIgnore]
    public string TimeText => CreatedAt.ToString("HH:mm");

    [JsonIgnore]
    public string DateText
    {
        get
        {
            var today = DateTime.Today;

            if (CreatedAt.Date == today)
                return "Bugün";

            if (CreatedAt.Date == today.AddDays(-1))
                return "Dün";

            return CreatedAt.ToString("dd.MM.yyyy");
        }
    }

    [JsonIgnore]
    public string SizeText => FormatBytes(TotalBytes);

    [JsonIgnore]
    public string FileCountText
    {
        get
        {
            if (FileCount <= 1)
                return "1 dosya";

            return $"{FileCount} dosya";
        }
    }

    [JsonIgnore]
    public string SummaryText =>
        $"{DirectionText} • {FileCountText} • {SizeText} • {TimeText}";

    [JsonIgnore]
    public Color AccentColor
    {
        get
        {
            if (!IsSuccess)
                return Color.FromArgb("#FF6B6B");

            return Direction == "receive"
                ? Color.FromArgb("#58D68D")
                : Color.FromArgb("#2D6BFF");
        }
    }

    [JsonIgnore]
    public Color AccentBackgroundColor
    {
        get
        {
            if (!IsSuccess)
                return Color.FromArgb("#351818");

            return Direction == "receive"
                ? Color.FromArgb("#16301F")
                : Color.FromArgb("#172642");
        }
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F1} KB";

        if (bytes < 1024L * 1024L * 1024L)
            return $"{bytes / 1024.0 / 1024.0:F1} MB";

        return $"{bytes / 1024.0 / 1024.0 / 1024.0:F1} GB";
    }
}