namespace RavenMobile.Features.WifiQr.Models;

public class RavenQrPayload
{
    public string App { get; set; } = "raven";
    public int Version { get; set; } = 1;

    public string Ssid { get; set; } = "";
    public string Password { get; set; } = "";

    public string Host { get; set; } = "";
    public int Port { get; set; } = 50555;
}