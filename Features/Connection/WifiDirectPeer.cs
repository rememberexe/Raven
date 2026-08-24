namespace RavenMobile.Features.Connection;

public class WifiDirectPeer
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";

    // Android WifiP2pDevice status:
    // 0 = Connected
    // 1 = Invited
    // 2 = Failed
    // 3 = Available
    // 4 = Unavailable
    public int Status { get; set; } = -1;

    public bool IsAvailable => Status == 3;

    public string StatusText => Status switch
    {
        0 => "Connected",
        1 => "Invited",
        2 => "Failed",
        3 => "Available",
        4 => "Unavailable",
        _ => "Unknown"
    };
}