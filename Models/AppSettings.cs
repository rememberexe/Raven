namespace RavenMobile.Models;

public class AppSettings
{
    public bool VibrateOnTransferComplete { get; set; } = true;

    public bool AutoClearFilesAfterSend { get; set; } = true;

    public bool AutoStopReceiveAfterTransfer { get; set; } = false;
}