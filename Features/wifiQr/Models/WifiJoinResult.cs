namespace RavenMobile.Features.WifiQr.Models;

public class WifiJoinResult
{
    public bool IsConnected { get; set; }
    public string Ssid { get; set; } = "";
    public string GatewayAddress { get; set; } = "";
    public string LocalAddress { get; set; } = "";
}