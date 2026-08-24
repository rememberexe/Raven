using RavenMobile.Features.Transfer.Model;

namespace RavenMobile.Features.WifiQr;

public interface IWifiQrTransferService
{
    event Action<string>? OnStatusChanged;
    event Action<TransferProgress>? OnProgressChanged;

    Task StartReceiverAsync();
    Task StopAsync();
}