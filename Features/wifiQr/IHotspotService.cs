using RavenMobile.Features.WifiQr.Models;

namespace RavenMobile.Features.WifiQr;

public interface IHotspotService
{
    event Action<string>? OnStatusChanged;

    Task<HotspotSessionInfo> StartHotspotAsync();

    Task StopHotspotAsync();
}