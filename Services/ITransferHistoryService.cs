using RavenMobile.Models;

namespace RavenMobile.Services;

public interface ITransferHistoryService
{
    Task<IReadOnlyList<TransferHistoryItem>> GetAllAsync();

    Task AddAsync(TransferHistoryItem item);

    Task ClearAsync();
}