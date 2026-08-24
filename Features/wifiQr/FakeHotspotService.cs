using RavenMobile.Features.WifiQr.Models;

namespace RavenMobile.Features.WifiQr;

#pragma warning disable CS0067

public class FakeHotspotService : IHotspotService
{
    public event Action<string>? OnStatusChanged;

    public Task<HotspotSessionInfo> StartHotspotAsync()
    {
        OnStatusChanged?.Invoke("Hotspot sadece Android cihazda destekleniyor.");

        return Task.FromResult(new HotspotSessionInfo
        {
            Ssid = "Raven_Test",
            Password = "12345678",
            HostAddress = "192.168.43.1",
            Port = 50555
        });
    }

    public Task StopHotspotAsync()
    {
        OnStatusChanged?.Invoke("Hotspot kapatıldı.");
        return Task.CompletedTask;
    }
}

#pragma warning restore CS0067