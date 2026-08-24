using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace RavenMobile.Models;

public class MenuItemModel : INotifyPropertyChanged
{
    private bool _isActive;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Key { get; set; } = "";

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
                return;

            _isActive = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(CardBackgroundColor));
            OnPropertyChanged(nameof(CardStrokeColor));
            OnPropertyChanged(nameof(IconBackgroundColor));
            OnPropertyChanged(nameof(TitleColor));
            OnPropertyChanged(nameof(SubtitleColor));
            OnPropertyChanged(nameof(ActiveBarColor));
        }
    }

    public Color CardBackgroundColor =>
        IsActive ? Color.FromArgb("#1A2C52") : Color.FromArgb("#B810141D");

    public Color CardStrokeColor =>
        IsActive ? Color.FromArgb("#2D6BFF") : Color.FromArgb("#26314A");

    public Color IconBackgroundColor =>
        IsActive ? Color.FromArgb("#203C72") : Color.FromArgb("#17213A");

    public Color TitleColor =>
        IsActive ? Colors.White : Color.FromArgb("#E8ECF5");

    public Color SubtitleColor =>
        IsActive ? Color.FromArgb("#9FB8FF") : Color.FromArgb("#7F8AA3");

    public Color ActiveBarColor =>
        IsActive ? Color.FromArgb("#2D6BFF") : Color.FromArgb("#002D6BFF");

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}