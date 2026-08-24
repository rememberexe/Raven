using RavenMobile.Features.WifiQr.Models;

namespace RavenMobile.Features.WifiQr;

public interface IWifiJoinService
{
    event Action<string>? OnStatusChanged;

    Task<WifiJoinResult> ConnectAsync(string ssid, string password);

    Task DisconnectAsync();
}