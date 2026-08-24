using RavenMobile.Features.Transfer.Model;

namespace RavenMobile.Features.WifiQr;

public interface IWifiQrSenderService
{
    event Action<string>? OnStatusChanged;
    event Action<TransferProgress>? OnProgressChanged;

    Task SendFilesAsync(
        string hostAddress,
        int port,
        IReadOnlyList<SelectedFileItem> files);
}