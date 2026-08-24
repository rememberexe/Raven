using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RavenMobile.Services;

namespace RavenMobile.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly IAppSettingsService _settingsService;
    private readonly ITransferHistoryService _historyService;

    private bool _vibrateOnTransferComplete;
    private bool _autoClearFilesAfterSend;
    private bool _autoStopReceiveAfterTransfer;

    private string _statusText = "Ayarlar hazır.";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand ResetOnboardingCommand { get; }
    public ICommand ClearHistoryCommand { get; }

    public SettingsViewModel(
        IAppSettingsService settingsService,
        ITransferHistoryService historyService)
    {
        _settingsService = settingsService;
        _historyService = historyService;

        var settings = _settingsService.GetSettings();

        _vibrateOnTransferComplete = settings.VibrateOnTransferComplete;
        _autoClearFilesAfterSend = settings.AutoClearFilesAfterSend;
        _autoStopReceiveAfterTransfer = settings.AutoStopReceiveAfterTransfer;

        ResetOnboardingCommand = new Command(ResetOnboarding);
        ClearHistoryCommand = new Command(async () => await ClearHistoryAsync());
    }

    public bool VibrateOnTransferComplete
    {
        get => _vibrateOnTransferComplete;
        set
        {
            if (_vibrateOnTransferComplete == value)
                return;

            _vibrateOnTransferComplete = value;
            _settingsService.SetVibrateOnTransferComplete(value);

            StatusText = value
                ? "Transfer bitince titreşim açıldı."
                : "Transfer bitince titreşim kapatıldı.";

            OnPropertyChanged();
        }
    }

    public bool AutoClearFilesAfterSend
    {
        get => _autoClearFilesAfterSend;
        set
        {
            if (_autoClearFilesAfterSend == value)
                return;

            _autoClearFilesAfterSend = value;
            _settingsService.SetAutoClearFilesAfterSend(value);

            StatusText = value
                ? "Gönderim bitince dosya listesi temizlenecek."
                : "Gönderim bitince dosya listesi korunacak.";

            OnPropertyChanged();
        }
    }

    public bool AutoStopReceiveAfterTransfer
    {
        get => _autoStopReceiveAfterTransfer;
        set
        {
            if (_autoStopReceiveAfterTransfer == value)
                return;

            _autoStopReceiveAfterTransfer = value;
            _settingsService.SetAutoStopReceiveAfterTransfer(value);

            StatusText = value
                ? "Alıcı modu transfer bitince otomatik kapanacak."
                : "Alıcı modu transfer bitince açık kalacak.";

            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText == value)
                return;

            _statusText = value;
            OnPropertyChanged();
        }
    }

    private void ResetOnboarding()
    {
        _settingsService.ResetOnboarding();

        StatusText = "Tanıtım ekranı sıfırlandı. Uygulamayı kapatıp açınca tekrar gösterilecek.";
    }

    private async Task ClearHistoryAsync()
    {
        await _historyService.ClearAsync();

        StatusText = "Transfer geçmişi temizlendi.";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}