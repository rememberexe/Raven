namespace RavenMobile.Features.Connection;

public class WifiDirectConnectionInfo
{
    public bool IsConnected { get; set; }
    public bool IsGroupOwner { get; set; }
    public string GroupOwnerAddress { get; set; } = "";
}