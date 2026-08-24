namespace RavenMobile.Features.Connection;

#pragma warning disable CS0067

public class FakeWifiDirectService : IWifiDirectService
{
    public event Action<WifiDirectPeer>? OnPeerFound;
    public event Action<WifiDirectPeer>? OnRavenDeviceFound;
    public event Action<string>? OnConnectionStatusChanged;
    public event Action<WifiDirectConnectionInfo>? ConnectionInfoAvailable;

    public Task RegisterRavenServiceAsync()
    {
        return Task.CompletedTask;
    }

    public Task DiscoverPeersAsync()
    {
        return Task.CompletedTask;
    }

    public Task DiscoverRavenDevicesAsync()
    {
        return Task.CompletedTask;
    }

    public Task<bool> StartConnectionAsync(string deviceAddress)
    {
        OnConnectionStatusChanged?.Invoke("Wi-Fi Direct bu platformda desteklenmiyor.");
        return Task.FromResult(false);
    }

    public Task RequestConnectionInfoAsync()
    {
        return Task.CompletedTask;
    }

    public Task ResetConnectionAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        return Task.CompletedTask;
    }
}

#pragma warning restore CS0067