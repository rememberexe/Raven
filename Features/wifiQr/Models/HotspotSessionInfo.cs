namespace RavenMobile.Features.WifiQr.Models;

public class HotspotSessionInfo
{
    public string Ssid { get; set; } = "";
    public string Password { get; set; } = "";
    public string HostAddress { get; set; } = "192.168.43.1";
    public int Port { get; set; } = 50555;
}