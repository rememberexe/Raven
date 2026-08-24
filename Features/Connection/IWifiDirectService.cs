namespace RavenMobile.Features.Connection;

public interface IWifiDirectService
{
    event Action<WifiDirectPeer>? OnPeerFound;
    event Action<WifiDirectPeer>? OnRavenDeviceFound;
    event Action<string>? OnConnectionStatusChanged;
    event Action<WifiDirectConnectionInfo>? ConnectionInfoAvailable;

    Task RegisterRavenServiceAsync();
    Task DiscoverPeersAsync();
    Task DiscoverRavenDevicesAsync();

    Task<bool> StartConnectionAsync(string deviceAddress);

    Task RequestConnectionInfoAsync();

    Task ResetConnectionAsync();
    Task DisconnectAsync();
}