using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RavenMobile.Models;
using RavenMobile.Services;

namespace RavenMobile.ViewModels;

public class HistoryViewModel : INotifyPropertyChanged
{
    private readonly ITransferHistoryService _historyService;

    private bool _isBusy;
    private string _historySummary = "Henüz kayıt yok";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TransferHistoryItem> Items { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand ClearCommand { get; }

    public HistoryViewModel(ITransferHistoryService historyService)
    {
        _historyService = historyService;

        LoadCommand = new Command(async () => await LoadAsync());
        ClearCommand = new Command(async () => await ClearAsync());
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value)
                return;

            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public bool HasHistory => Items.Count > 0;

    public bool IsEmpty => Items.Count == 0;

    public string HistorySummary
    {
        get => _historySummary;
        set
        {
            if (_historySummary == value)
                return;

            _historySummary = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            var items = await _historyService.GetAllAsync();

            Items.Clear();

            foreach (var item in items)
                Items.Add(item);

            UpdateState();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearAsync()
    {
        await _historyService.ClearAsync();

        Items.Clear();

        UpdateState();
    }

    private void UpdateState()
    {
        HistorySummary = Items.Count == 0
            ? "Henüz kayıt yok"
            : $"{Items.Count} transfer kaydı";

        OnPropertyChanged(nameof(HasHistory));
        OnPropertyChanged(nameof(IsEmpty));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}