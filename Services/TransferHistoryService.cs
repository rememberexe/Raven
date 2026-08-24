using System.Text.Json;
using RavenMobile.Models;

namespace RavenMobile.Services;

public class TransferHistoryService : ITransferHistoryService
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public TransferHistoryService()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "transfer_history.json");
    }

    public async Task<IReadOnlyList<TransferHistoryItem>> GetAllAsync()
    {
        await _lock.WaitAsync();

        try
        {
            var items = await ReadInternalAsync();

            return items
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(TransferHistoryItem item)
    {
        await _lock.WaitAsync();

        try
        {
            var items = await ReadInternalAsync();

            item.Id = string.IsNullOrWhiteSpace(item.Id)
                ? Guid.NewGuid().ToString("N")
                : item.Id;

            if (item.CreatedAt == default)
                item.CreatedAt = DateTime.Now;

            items.Insert(0, item);

            items = items
                .OrderByDescending(x => x.CreatedAt)
                .Take(250)
                .ToList();

            await WriteInternalAsync(items);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ClearAsync()
    {
        await _lock.WaitAsync();

        try
        {
            await WriteInternalAsync(new List<TransferHistoryItem>());
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<TransferHistoryItem>> ReadInternalAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new List<TransferHistoryItem>();

            var json = await File.ReadAllTextAsync(_filePath);

            if (string.IsNullOrWhiteSpace(json))
                return new List<TransferHistoryItem>();

            return JsonSerializer.Deserialize<List<TransferHistoryItem>>(json, _jsonOptions)
                   ?? new List<TransferHistoryItem>();
        }
        catch
        {
            return new List<TransferHistoryItem>();
        }
    }

    private async Task WriteInternalAsync(List<TransferHistoryItem> items)
    {
        var json = JsonSerializer.Serialize(items, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}