using RavenMobile.Features.Transfer.Model;

namespace RavenMobile.Features.Transfer;

public interface ITransferService
{
    event Action<string>? OnTransferStatusChanged;
    event Action<TransferProgress>? OnTransferProgressChanged;
    event Func<IncomingTransferRequest, Task<bool>>? OnIncomingTransferRequest;
    event Action? OnTransferSessionFinished;

    Task StartGroupOwnerSessionAsync(IReadOnlyList<SelectedFileItem> pendingFiles);
    Task ConnectToGroupOwnerSessionAsync(string groupOwnerAddress, IReadOnlyList<SelectedFileItem> pendingFiles);
    Task StopSessionAsync();
}