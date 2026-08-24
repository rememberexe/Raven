using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace RavenMobile.Features.Transfer.Model;

public class SelectedFileItem
{
    public string FileName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public long Size { get; set; }

    public string SizeText
    {
        get
        {
            if (Size < 1024)
                return $"{Size} B";

            if (Size < 1024 * 1024)
                return $"{Size / 1024.0:F1} KB";

            if (Size < 1024 * 1024 * 1024)
                return $"{Size / 1024.0 / 1024.0:F1} MB";

            return $"{Size / 1024.0 / 1024.0 / 1024.0:F1} GB";
        }
    }

    public string Extension
    {
        get
        {
            var ext = Path.GetExtension(FileName);

            if (string.IsNullOrWhiteSpace(ext))
                ext = Path.GetExtension(FullPath);

            return ext?.ToLowerInvariant() ?? "";
        }
    }

    public bool IsImageFile => Extension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" or ".heic" => true,
        _ => false
    };

    public bool HasImagePreview =>
     AllowPreview &&
     IsImageFile &&
     !string.IsNullOrWhiteSpace(FullPath) &&
     File.Exists(FullPath);

    public bool ShowFileIcon => !HasImagePreview;
    public bool AllowPreview { get; set; } = true;
    public ImageSource? PreviewSource =>
        HasImagePreview ? ImageSource.FromFile(FullPath) : null;

    public string FileIcon => Extension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" or ".heic" => "🖼️",
        ".mp4" or ".mkv" or ".avi" or ".mov" or ".webm" or ".3gp" => "🎬",
        ".mp3" or ".wav" or ".aac" or ".flac" or ".ogg" or ".m4a" => "🎵",
        ".pdf" => "📕",
        ".doc" or ".docx" => "📘",
        ".xls" or ".xlsx" or ".csv" => "📊",
        ".ppt" or ".pptx" => "📙",
        ".txt" or ".log" or ".md" => "📄",
        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "🗜️",
        ".apk" => "🤖",
        ".exe" or ".msi" => "💻",
        ".cs" or ".xaml" or ".xml" or ".json" or ".html" or ".css" or ".js" or ".py" or ".java" or ".cpp" or ".c" => "🧩",
        ".iso" => "💿",
        _ => "📦"
    };

    public string FileTypeText => Extension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" or ".heic" => "Görsel",
        ".mp4" or ".mkv" or ".avi" or ".mov" or ".webm" or ".3gp" => "Video",
        ".mp3" or ".wav" or ".aac" or ".flac" or ".ogg" or ".m4a" => "Ses",
        ".pdf" => "PDF",
        ".doc" or ".docx" => "Word",
        ".xls" or ".xlsx" or ".csv" => "Tablo",
        ".ppt" or ".pptx" => "Sunum",
        ".txt" or ".log" or ".md" => "Metin",
        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "Arşiv",
        ".apk" => "Android APK",
        ".exe" or ".msi" => "Windows uygulaması",
        ".cs" or ".xaml" or ".xml" or ".json" or ".html" or ".css" or ".js" or ".py" or ".java" or ".cpp" or ".c" => "Kod dosyası",
        ".iso" => "Disk imajı",
        _ => "Dosya"
    };

    public Color FileAccentColor => Extension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" or ".heic" => Color.FromArgb("#8A4DFF"),
        ".mp4" or ".mkv" or ".avi" or ".mov" or ".webm" or ".3gp" => Color.FromArgb("#FF5C5C"),
        ".mp3" or ".wav" or ".aac" or ".flac" or ".ogg" or ".m4a" => Color.FromArgb("#58D68D"),
        ".pdf" => Color.FromArgb("#FF4D4D"),
        ".doc" or ".docx" => Color.FromArgb("#4D8DFF"),
        ".xls" or ".xlsx" or ".csv" => Color.FromArgb("#3DDC84"),
        ".ppt" or ".pptx" => Color.FromArgb("#FF9F43"),
        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => Color.FromArgb("#F5C542"),
        ".apk" => Color.FromArgb("#58D68D"),
        ".cs" or ".xaml" or ".xml" or ".json" or ".html" or ".css" or ".js" or ".py" or ".java" or ".cpp" or ".c" => Color.FromArgb("#00E0FF"),
        _ => Color.FromArgb("#2D6BFF")
    };

    public Color FileAccentBackgroundColor => Extension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" or ".heic" => Color.FromArgb("#231A3A"),
        ".mp4" or ".mkv" or ".avi" or ".mov" or ".webm" or ".3gp" => Color.FromArgb("#351A1A"),
        ".mp3" or ".wav" or ".aac" or ".flac" or ".ogg" or ".m4a" => Color.FromArgb("#16301F"),
        ".pdf" => Color.FromArgb("#351818"),
        ".doc" or ".docx" => Color.FromArgb("#172642"),
        ".xls" or ".xlsx" or ".csv" => Color.FromArgb("#16301F"),
        ".ppt" or ".pptx" => Color.FromArgb("#352618"),
        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => Color.FromArgb("#302817"),
        ".apk" => Color.FromArgb("#16301F"),
        ".cs" or ".xaml" or ".xml" or ".json" or ".html" or ".css" or ".js" or ".py" or ".java" or ".cpp" or ".c" => Color.FromArgb("#112B34"),
        _ => Color.FromArgb("#1B2947")
    };
}