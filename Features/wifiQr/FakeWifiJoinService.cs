using RavenMobile.Features.WifiQr.Models;

namespace RavenMobile.Features.WifiQr;

#pragma warning disable CS0067

public class FakeWifiJoinService : IWifiJoinService
{
    public event Action<string>? OnStatusChanged;

    public Task<WifiJoinResult> ConnectAsync(string ssid, string password)
    {
        OnStatusChanged?.Invoke("Wi-Fi bağlantısı sadece Android cihazda destekleniyor.");

        return Task.FromResult(new WifiJoinResult
        {
            IsConnected = false,
            Ssid = ssid,
            GatewayAddress = "192.168.43.1"
        });
    }

    public Task DisconnectAsync()
    {
        OnStatusChanged?.Invoke("Wi-Fi bağlantısı kapatıldı.");
        return Task.CompletedTask;
    }
}

#pragma warning restore CS0067