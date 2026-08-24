using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RavenMobile.Features.Discovery.Model;

public class DiscoveredDevice : INotifyPropertyChanged
{
    private string _name = "Unknown Device";
    private string _address = "";
    private string _wifiDirectAddress = "";
    private int _wifiDirectStatus = -1;
    private DateTime _lastSeen = DateTime.Now;
    private RavenDeviceType _deviceType = RavenDeviceType.Unknown;
    private DeviceConnectionState _connectionState = DeviceConnectionState.Nearby;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Address
    {
        get => _address;
        set { _address = value; OnPropertyChanged(); }
    }

    public string WifiDirectAddress
    {
        get => _wifiDirectAddress;
        set { _wifiDirectAddress = value; OnPropertyChanged(); }
    }

    public int WifiDirectStatus
    {
        get => _wifiDirectStatus;
        set
        {
            _wifiDirectStatus = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WifiDirectStatusText));
        }
    }

    public string WifiDirectStatusText => WifiDirectStatus switch
    {
        0 => "Connected",
        1 => "Invited",
        2 => "Failed",
        3 => "Available",
        4 => "Unavailable",
        _ => "Unknown"
    };

    public DateTime LastSeen
    {
        get => _lastSeen;
        set { _lastSeen = value; OnPropertyChanged(); }
    }

    public RavenDeviceType DeviceType
    {
        get => _deviceType;
        set
        {
            _deviceType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Icon));
        }
    }

    public DeviceConnectionState ConnectionState
    {
        get => _connectionState;
        set
        {
            _connectionState = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusBackgroundColor));
        }
    }

    public bool CanConnect =>
        ConnectionState == DeviceConnectionState.Connectable &&
        !string.IsNullOrWhiteSpace(WifiDirectAddress);

    public string Icon => DeviceType switch
    {
        RavenDeviceType.Phone => "📱",
        RavenDeviceType.Computer => "💻",
        RavenDeviceType.Headset => "🎧",
        RavenDeviceType.Watch => "⌚",
        RavenDeviceType.Tv => "📺",
        _ => "📡"
    };

    public string StatusText => ConnectionState switch
    {
        DeviceConnectionState.Found => "Bulundu",
        DeviceConnectionState.Connectable => "Bağlanabilir",
        DeviceConnectionState.Invited => "Davet bekliyor",
        DeviceConnectionState.Connecting => "Bağlanıyor",
        DeviceConnectionState.Connected => "Bağlandı",
        DeviceConnectionState.Unavailable => "Hazır değil",
        DeviceConnectionState.RavenReady => "Raven Hazır",
        _ => "Yakında"
    };

    public Color StatusColor => ConnectionState switch
    {
        DeviceConnectionState.Found => Color.FromArgb("#B0B0B0"),
        DeviceConnectionState.Connectable => Color.FromArgb("#58D68D"),
        DeviceConnectionState.Invited => Color.FromArgb("#F5C542"),
        DeviceConnectionState.Connecting => Color.FromArgb("#5DADE2"),
        DeviceConnectionState.Connected => Color.FromArgb("#2D6BFF"),
        DeviceConnectionState.Unavailable => Color.FromArgb("#FF6B6B"),
        DeviceConnectionState.RavenReady => Color.FromArgb("#58D68D"),
        _ => Color.FromArgb("#A0A0A0")
    };

    public Color StatusBackgroundColor => ConnectionState switch
    {
        DeviceConnectionState.Found => Color.FromArgb("#24242A"),
        DeviceConnectionState.Connectable => Color.FromArgb("#1F2C1F"),
        DeviceConnectionState.Invited => Color.FromArgb("#2E2814"),
        DeviceConnectionState.Connecting => Color.FromArgb("#142433"),
        DeviceConnectionState.Connected => Color.FromArgb("#14234D"),
        DeviceConnectionState.Unavailable => Color.FromArgb("#341919"),
        DeviceConnectionState.RavenReady => Color.FromArgb("#1F2C1F"),
        _ => Color.FromArgb("#24242A")
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}