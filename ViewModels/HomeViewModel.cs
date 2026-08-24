using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using QRCoder;
using RavenMobile.Features.Transfer.Model;
using RavenMobile.Features.WifiQr;
using RavenMobile.Features.WifiQr.Models;
using RavenMobile.Views;
using RavenMobile.Models;
using RavenMobile.Services;
namespace RavenMobile.ViewModels;

public class HomeViewModel : INotifyPropertyChanged
{
    private readonly IHotspotService _hotspot;
    private readonly IWifiQrTransferService _receiver;
    private readonly IWifiJoinService _wifiJoin;
    private readonly IWifiQrSenderService _sender;

    private string _statusText = "Raven hazır.";
    private bool _isReceivePanelVisible;
    private bool _isTransferPanelVisible;
    private bool _isHotspotActive;
    private bool _isBusy;

    private ImageSource? _qrImageSource;
    private readonly IAppSettingsService _appSettingsService;
    private string _hotspotSsid = "";
    private string _hotspotPassword = "";
    private string _hotspotAddress = "";
    private readonly ITransferHistoryService _historyService;
    private bool _historySavedForCurrentTransfer;
    private double _transferProgress;
    private string _transferProgressText = "";
    private string _transferSpeedText = "";
    private string _transferRemainingText = "";
    private string _transferCurrentFileName = "";
    private string _transferSubtitle = "";
    private string _transferBytesText = "";
    private string _statusIcon = "●";
    private Color _statusAccentColor = Color.FromArgb("#2D6BFF");
    private Color _statusCardBackgroundColor = Color.FromArgb("#0F121B");
    private Color _statusIconBackgroundColor = Color.FromArgb("#172642");
    private bool _isCompletionPanelVisible;
    private string _completionIcon = "✅";
    private string _completionTitle = "";
    private string _completionMessage = "";
    private string _completionDetails = "";
    private Color _completionAccentColor = Color.FromArgb("#58D68D");
    private const int MaxVisibleSelectedFiles = 250;
    private const int MaxPreviewEnabledFiles = 80;

    private readonly List<SelectedFileItem> _allSelectedFiles = new();
    private long _lastTransferredBytes;
    private long _lastTotalBytes;
    private int _lastFileCount;
    private string _lastTransferDirection = "Transfer";
    public ObservableCollection<SelectedFileItem> SelectedFiles { get; } = new();

    public ICommand StartReceiveCommand { get; }
    public ICommand StopReceiveCommand { get; }
    public ICommand PickFileCommand { get; }
    public ICommand StartSendCommand { get; }
    public ICommand ClearFilesCommand { get; }
    public ICommand DismissCompletionCommand { get; }
    public HomeViewModel(
     IHotspotService hotspot,
     IWifiQrTransferService receiver,
     IWifiJoinService wifiJoin,
     IWifiQrSenderService sender,
     ITransferHistoryService historyService,
     IAppSettingsService appSettingsService)
    {
        _hotspot = hotspot;
        _receiver = receiver;
        _wifiJoin = wifiJoin;
        _sender = sender;
        _historyService = historyService;
        _appSettingsService = appSettingsService;
        StartReceiveCommand = new Command(async () => await StartReceiveAsync());
        StopReceiveCommand = new Command(async () => await StopReceiveAsync());
        PickFileCommand = new Command(async () => await PickFileAsync());
        StartSendCommand = new Command(async () => await StartSendAsync());
        ClearFilesCommand = new Command(ClearSelectedFiles);
        //DismissCompletionCommand = new Command(() => IsCompletionPanelVisible = false);
        DismissCompletionCommand = new Command(DismissCompletionPanel);
        _hotspot.OnStatusChanged += status =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusText = status;
            });
        };

        _receiver.OnStatusChanged += status =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusText = status;

                if (IsSuccessStatus(status))
                    ShowCompletionPanel(true, "Alım", status);

                if (IsErrorStatus(status))
                    ShowCompletionPanel(false, "Alım", status);
            });
        };

        _receiver.OnProgressChanged += progress =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateProgress(progress, "Alım");
            });
        };

        _wifiJoin.OnStatusChanged += status =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusText = status;
            });
        };

        _sender.OnStatusChanged += status =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusText = status;

                if (IsSuccessStatus(status))
                    ShowCompletionPanel(true, "Gönderim", status);

                if (IsErrorStatus(status))
                    ShowCompletionPanel(false, "Gönderim", status);
            });
        };

        _sender.OnProgressChanged += progress =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateProgress(progress, "Gönderim");
            });
        };
    }
    private void DismissCompletionPanel()
    {
        IsCompletionPanelVisible = false;
        IsTransferPanelVisible = false;

        TransferProgress = 0;
        TransferProgressText = "";
        TransferSpeedText = "";
        TransferRemainingText = "";
        TransferCurrentFileName = "";
        TransferSubtitle = "";
        TransferBytesText = "";

        SetKeepScreenOn(false);
    }
    public string StatusIcon
    {
        get => _statusIcon;
        set
        {
            _statusIcon = value;
            OnPropertyChanged();
        }
    }

    public Color StatusAccentColor
    {
        get => _statusAccentColor;
        set
        {
            _statusAccentColor = value;
            OnPropertyChanged();
        }
    }

    public Color StatusCardBackgroundColor
    {
        get => _statusCardBackgroundColor;
        set
        {
            _statusCardBackgroundColor = value;
            OnPropertyChanged();
        }
    }

    public Color StatusIconBackgroundColor
    {
        get => _statusIconBackgroundColor;
        set
        {
            _statusIconBackgroundColor = value;
            OnPropertyChanged();
        }
    }

    private static void SetKeepScreenOn(bool keepOn)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DeviceDisplay.Current.KeepScreenOn = keepOn;
            });
        }
        catch
        {
        }
    }
    public string SelectedFilesTotalSize
    {
        get
        {
            var total = _allSelectedFiles.Sum(x => x.Size);
            return FormatBytes(total);
        }
    }

    public string TransferSubtitle
    {
        get => _transferSubtitle;
        set
        {
            _transferSubtitle = value;
            OnPropertyChanged();
        }
    }

    public string TransferBytesText
    {
        get => _transferBytesText;
        set
        {
            _transferBytesText = value;
            OnPropertyChanged();
        }
    }

    public bool IsCompletionPanelVisible
    {
        get => _isCompletionPanelVisible;
        set
        {
            _isCompletionPanelVisible = value;
            OnPropertyChanged();
        }
    }

    public string CompletionIcon
    {
        get => _completionIcon;
        set
        {
            _completionIcon = value;
            OnPropertyChanged();
        }
    }

    public string CompletionTitle
    {
        get => _completionTitle;
        set
        {
            _completionTitle = value;
            OnPropertyChanged();
        }
    }

    public string CompletionMessage
    {
        get => _completionMessage;
        set
        {
            _completionMessage = value;
            OnPropertyChanged();
        }
    }

    public string CompletionDetails
    {
        get => _completionDetails;
        set
        {
            _completionDetails = value;
            OnPropertyChanged();
        }
    }

    public Color CompletionAccentColor
    {
        get => _completionAccentColor;
        set
        {
            _completionAccentColor = value;
            OnPropertyChanged();
        }
    }
    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            UpdateStatusVisual(value);
            OnPropertyChanged();
        }
    }
    private void UpdateStatusVisual(string? status)
    {
        var text = status ?? "";

        if (text.Contains("hata", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("başarısız", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("açılamadı", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("bağlanılamadı", StringComparison.OrdinalIgnoreCase))
        {
            StatusIcon = "⚠️";
            StatusAccentColor = Color.FromArgb("#FF6B6B");
            StatusCardBackgroundColor = Color.FromArgb("#181016");
            StatusIconBackgroundColor = Color.FromArgb("#351818");
            return;
        }

        if (text.Contains("tamamlandı", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("başarıyla", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("kaydedildi", StringComparison.OrdinalIgnoreCase))
        {
            StatusIcon = "✅";
            StatusAccentColor = Color.FromArgb("#58D68D");
            StatusCardBackgroundColor = Color.FromArgb("#0F1813");
            StatusIconBackgroundColor = Color.FromArgb("#16301F");
            return;
        }

        if (text.Contains("bekleniyor", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("hazır", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("alıcı", StringComparison.OrdinalIgnoreCase))
        {
            StatusIcon = "📡";
            StatusAccentColor = Color.FromArgb("#F5C542");
            StatusCardBackgroundColor = Color.FromArgb("#18160F");
            StatusIconBackgroundColor = Color.FromArgb("#302817");
            return;
        }

        if (text.Contains("transfer", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("gönder", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("alım", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("bağlan", StringComparison.OrdinalIgnoreCase))
        {
            StatusIcon = "⚡";
            StatusAccentColor = Color.FromArgb("#2D6BFF");
            StatusCardBackgroundColor = Color.FromArgb("#0F1422");
            StatusIconBackgroundColor = Color.FromArgb("#172642");
            return;
        }

        StatusIcon = "●";
        StatusAccentColor = Color.FromArgb("#2D6BFF");
        StatusCardBackgroundColor = Color.FromArgb("#0F121B");
        StatusIconBackgroundColor = Color.FromArgb("#172642");
    }
    public bool IsReceivePanelVisible
    {
        get => _isReceivePanelVisible;
        set
        {
            _isReceivePanelVisible = value;
            OnPropertyChanged();
        }
    }

    public bool IsTransferPanelVisible
    {
        get => _isTransferPanelVisible;
        set
        {
            _isTransferPanelVisible = value;
            OnPropertyChanged();
        }
    }

    public ImageSource? QrImageSource
    {
        get => _qrImageSource;
        set
        {
            _qrImageSource = value;
            OnPropertyChanged();
        }
    }

    public string HotspotSsid
    {
        get => _hotspotSsid;
        set
        {
            _hotspotSsid = value;
            OnPropertyChanged();
        }
    }

    public string HotspotPassword
    {
        get => _hotspotPassword;
        set
        {
            _hotspotPassword = value;
            OnPropertyChanged();
        }
    }

    public string HotspotAddress
    {
        get => _hotspotAddress;
        set
        {
            _hotspotAddress = value;
            OnPropertyChanged();
        }
    }

    public string SelectedFilesSummary
    {
        get
        {
            if (_allSelectedFiles.Count == 0)
                return "Henüz dosya seçilmedi";

            var totalSize = _allSelectedFiles.Sum(x => x.Size);

            if (_allSelectedFiles.Count > MaxVisibleSelectedFiles)
            {
                return $"{_allSelectedFiles.Count} dosya seçildi • Listede ilk {MaxVisibleSelectedFiles} gösteriliyor • Tamamı gönderilecek";
            }

            return $"{_allSelectedFiles.Count} dosya seçildi • {FormatBytes(totalSize)}";
        }
    }

    public double TransferProgress
    {
        get => _transferProgress;
        set
        {
            _transferProgress = value;
            OnPropertyChanged();
        }
    }

    public string TransferProgressText
    {
        get => _transferProgressText;
        set
        {
            _transferProgressText = value;
            OnPropertyChanged();
        }
    }

    public string TransferSpeedText
    {
        get => _transferSpeedText;
        set
        {
            _transferSpeedText = value;
            OnPropertyChanged();
        }
    }

    public string TransferRemainingText
    {
        get => _transferRemainingText;
        set
        {
            _transferRemainingText = value;
            OnPropertyChanged();
        }
    }

    public string TransferCurrentFileName
    {
        get => _transferCurrentFileName;
        set
        {
            _transferCurrentFileName = value;
            OnPropertyChanged();
        }
    }

    private async Task StartReceiveAsync()
    {
        if (_isBusy)
            return;
        _historySavedForCurrentTransfer = false;
        if (_isHotspotActive)
        {
            StatusText = "Alıcı modu zaten aktif.";
            return;
        }

        try
        {
            _isBusy = true;

            ResetTransferPanel();

            StatusText = "Alıcı modu başlatılıyor...";

            await _receiver.StartReceiverAsync();

            var session = await _hotspot.StartHotspotAsync();

            HotspotSsid = session.Ssid;
            HotspotPassword = session.Password;
            HotspotAddress = $"{session.HostAddress}:{session.Port}";

            var payload = new RavenQrPayload
            {
                Ssid = session.Ssid,
                Password = session.Password,
                Host = session.HostAddress,
                Port = session.Port
            };

            QrImageSource = CreateQrImage(payload);

            _isHotspotActive = true;
            IsReceivePanelVisible = true;

            StatusText = "QR kod hazır. Gönderici bu kodu okutmalı.";
        }
        catch (Exception ex)
        {
            StatusText = $"Alıcı modu başlatılamadı: {ex.Message}";

            await _receiver.StopAsync();
            await _hotspot.StopHotspotAsync();

            _isHotspotActive = false;
            IsReceivePanelVisible = false;
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task StopReceiveAsync()
    {
        try
        {
            await _receiver.StopAsync();
            await _hotspot.StopHotspotAsync();

            _isHotspotActive = false;
            IsReceivePanelVisible = false;
            QrImageSource = null;

            StatusText = "Alıcı modu kapatıldı.";
        }
        catch (Exception ex)
        {
            StatusText = $"Alıcı modu kapatılamadı: {ex.Message}";
        }
    }

    private async Task StartSendAsync()
    {
        _historySavedForCurrentTransfer = false;
        if (_isBusy)
            return;

        if (_allSelectedFiles.Count == 0)
        {
            StatusText = "Göndermek için önce dosya seç.";
            return;
        }

        try
        {
            _isBusy = true;

            ResetTransferPanel();

            // ÖNEMLİ:
            // Bu cihaz daha önce AL modundaysa, önce kapatıyoruz.
            // Aynı anda hem hotspot açıp hem başka hotspot'a bağlanmaya çalışmasın.
            if (_isHotspotActive || IsReceivePanelVisible)
            {
                StatusText = "Alıcı modu kapatılıyor...";

                await _receiver.StopAsync();
                await _hotspot.StopHotspotAsync();

                _isHotspotActive = false;
                IsReceivePanelVisible = false;
                QrImageSource = null;

                await Task.Delay(1200);
            }

            // Önceki gönderimden kalan Wi-Fi bind/network varsa temizle.
            await _wifiJoin.DisconnectAsync();

            await Task.Delay(500);

            StatusText = "QR kod okutuluyor...";

            var qrText = await ScanQrAsync();

            if (string.IsNullOrWhiteSpace(qrText))
            {
                StatusText = "QR okutma iptal edildi.";
                return;
            }

            var payload = JsonSerializer.Deserialize<RavenQrPayload>(
                qrText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (payload == null ||
                !string.Equals(payload.App, "raven", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(payload.Ssid) ||
                string.IsNullOrWhiteSpace(payload.Password))
            {
                StatusText = "Geçersiz Raven QR kodu.";
                return;
            }

            StatusText = "Alıcı ağına bağlanılıyor...";

            var joinResult = await _wifiJoin.ConnectAsync(payload.Ssid, payload.Password);

            if (!joinResult.IsConnected)
            {
                StatusText = "Alıcı Wi-Fi ağına bağlanılamadı.";
                return;
            }

            // Öncelik QR içindeki host bilgisinde.
            // Çünkü QR'ı alıcı cihaz üretiyor.
            var host = !string.IsNullOrWhiteSpace(payload.Host)
                ? payload.Host
                : joinResult.GatewayAddress;

            if (string.IsNullOrWhiteSpace(host))
                host = "192.168.43.1";

            StatusText = $"Alıcıya dosya gönderiliyor: {host}:{payload.Port}";

            await _sender.SendFilesAsync(
                host,
                payload.Port,
                _allSelectedFiles.ToList());
        }
        catch (Exception ex)
        {
            StatusText = $"Gönderim başlatılamadı: {ex.Message}";
        }
        finally
        {
            // ÖNEMLİ:
            // Gönderim bitince alıcının Wi-Fi ağına bağlı kalmasın.
            // Böylece bu cihaz hemen AL moduna geçebilir.
            await _wifiJoin.DisconnectAsync();

            _isBusy = false;
        }
    }

    private async Task<string?> ScanQrAsync()
    {
        var resultSource = new TaskCompletionSource<string?>();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var navigationPage = GetActiveNavigationPage();

            if (navigationPage == null)
            {
                resultSource.TrySetResult(null);
                StatusText = "QR ekranı açılamadı: NavigationPage bulunamadı.";
                return;
            }

            await navigationPage.Navigation.PushAsync(new QrScannerPage(resultSource));
        });

        return await resultSource.Task;
    }
    private static NavigationPage? GetActiveNavigationPage()
    {
        var rootPage = Application.Current?.Windows.FirstOrDefault()?.Page;

        if (rootPage is NavigationPage navigationPage)
            return navigationPage;

        if (rootPage is FlyoutPage flyoutPage)
        {
            if (flyoutPage.Detail is NavigationPage detailNavigationPage)
                return detailNavigationPage;
        }

        return null;
    }
    private async Task PickFileAsync()
    {
        try
        {
            var results = await FilePicker.Default.PickMultipleAsync();

            if (results == null)
                return;

            var pickedFiles = results.ToList();

            if (pickedFiles.Count == 0)
                return;

            StatusText = "Dosyalar hazırlanıyor...";

            var allowPreview = pickedFiles.Count <= MaxPreviewEnabledFiles;

            var newItems = new List<SelectedFileItem>();

            foreach (var file in pickedFiles)
            {
                try
                {
                    var fullPath = file.FullPath ?? "";
                    var size = 0L;

                    if (!string.IsNullOrWhiteSpace(fullPath) && File.Exists(fullPath))
                    {
                        size = new FileInfo(fullPath).Length;
                    }

                    newItems.Add(new SelectedFileItem
                    {
                        FileName = file.FileName ?? "Dosya",
                        FullPath = fullPath,
                        Size = size,
                        AllowPreview = allowPreview
                    });
                }
                catch
                {
                    newItems.Add(new SelectedFileItem
                    {
                        FileName = file.FileName ?? "Dosya",
                        FullPath = file.FullPath ?? "",
                        Size = 0,
                        AllowPreview = false
                    });
                }
            }

            _allSelectedFiles.Clear();
            _allSelectedFiles.AddRange(newItems);

            SelectedFiles.Clear();

            foreach (var item in _allSelectedFiles.Take(MaxVisibleSelectedFiles))
            {
                SelectedFiles.Add(item);
            }

            OnPropertyChanged(nameof(SelectedFilesSummary));
            OnPropertyChanged(nameof(SelectedFilesTotalSize));

            if (_allSelectedFiles.Count > MaxVisibleSelectedFiles)
            {
                StatusText = $"{_allSelectedFiles.Count} dosya seçildi. Performans için listede ilk {MaxVisibleSelectedFiles} gösteriliyor, tamamı gönderilecek.";
            }
            else
            {
                StatusText = $"{_allSelectedFiles.Count} dosya seçildi.";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Dosya seçme hatası: {ex.Message}";
        }
    }
    private void ClearSelectedFiles()
    {
        _allSelectedFiles.Clear();
        SelectedFiles.Clear();

        OnPropertyChanged(nameof(SelectedFilesSummary));
        OnPropertyChanged(nameof(SelectedFilesTotalSize));

        StatusText = "Seçili dosyalar temizlendi.";
        IsCompletionPanelVisible = false;
    }

    private void UpdateProgress(TransferProgress progress, string direction)
    {
        SetKeepScreenOn(true);
        IsTransferPanelVisible = true;
        IsCompletionPanelVisible = false;

        _lastTransferDirection = direction;
        _lastTransferredBytes = progress.SentBytes;
        _lastTotalBytes = progress.TotalBytes;
        _lastFileCount = progress.TotalFiles;

        TransferProgress = progress.Percent / 100.0;
        TransferProgressText = $"{progress.Percent:F0}%";

        TransferSubtitle = $"{direction} devam ediyor • {progress.CurrentFileIndex}/{progress.TotalFiles} dosya";
        TransferBytesText = $"{FormatBytes(progress.SentBytes)} / {FormatBytes(progress.TotalBytes)}";

        TransferSpeedText = progress.SpeedText;
        TransferRemainingText = progress.RemainingTimeText;

        TransferCurrentFileName =
            $"{progress.CurrentFileIndex}/{progress.TotalFiles} - {progress.CurrentFileName}";
    }

    private void ResetTransferPanel()
    {
        SetKeepScreenOn(false);
        IsTransferPanelVisible = false;
        IsCompletionPanelVisible = false;

        TransferProgress = 0;
        TransferProgressText = "";
        TransferSpeedText = "";
        TransferRemainingText = "";
        TransferCurrentFileName = "";
        TransferSubtitle = "";
        TransferBytesText = "";

        _lastTransferredBytes = 0;
        _lastTotalBytes = 0;
        _lastFileCount = 0;
        _lastTransferDirection = "Transfer";
    }

    private void ShowCompletionPanel(bool success, string direction, string status)
    {
        IsTransferPanelVisible = false;
        SetKeepScreenOn(false);
        IsCompletionPanelVisible = true;

        if (success)
        {
            CompletionIcon = direction == "Alım" ? "📥" : "📤";
            CompletionTitle = $"{direction} tamamlandı";
            CompletionMessage = direction == "Alım"
                ? "Dosyalar başarıyla alındı ve kaydedildi."
                : "Dosyalar başarıyla gönderildi.";

            CompletionAccentColor = Color.FromArgb("#58D68D");
        }
        else
        {
            CompletionIcon = "⚠️";
            CompletionTitle = $"{direction} hatası";
            CompletionMessage = status;
            CompletionAccentColor = Color.FromArgb("#FF6B6B");
        }

        var fileText = _lastFileCount <= 0
            ? "Dosya bilgisi yok"
            : $"{_lastFileCount} dosya";

        var sizeText = _lastTotalBytes <= 0
            ? ""
            : $" • {FormatBytes(_lastTotalBytes)}";

        CompletionDetails = $"{fileText}{sizeText}";

        _ = SaveHistoryIfNeededAsync(success, direction, status);
        ApplySettingsAfterTransferIfNeeded(success, direction);
    }
    private async void ApplySettingsAfterTransferIfNeeded(bool success, string direction)
    {
        var settings = _appSettingsService.GetSettings();

        if (settings.VibrateOnTransferComplete)
        {
            VibrateTransferResult(success);
        }

        if (!success)
            return;

        if (direction == "Gönderim" && settings.AutoClearFilesAfterSend)
        {
            await Task.Delay(500);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _allSelectedFiles.Clear();
                SelectedFiles.Clear();

                OnPropertyChanged(nameof(SelectedFilesSummary));
                OnPropertyChanged(nameof(SelectedFilesTotalSize));

                StatusText = "Gönderim tamamlandı. Seçili dosyalar temizlendi.";
            });
        }

        if (direction == "Alım" && settings.AutoStopReceiveAfterTransfer)
        {
            await Task.Delay(900);

            await StopReceiveModeAfterTransferAsync();
        }
    }

    private static void VibrateTransferResult(bool success)
    {
        try
        {
            if (success)
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(180));
            }
            else
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(90));

                Task.Delay(130).ContinueWith(_ =>
                {
                    try
                    {
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(90));
                    }
                    catch
                    {
                    }
                });
            }
        }
        catch
        {
        }
    }

    private async Task StopReceiveModeAfterTransferAsync()
    {
        try
        {
            await _receiver.StopAsync();
            await _hotspot.StopHotspotAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsReceivePanelVisible = false;
                QrImageSource = null;

                HotspotSsid = "";
                HotspotPassword = "";
                HotspotAddress = "";

                StatusText = "Alım tamamlandı. Alıcı modu otomatik kapatıldı.";
            });
        }
        catch
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusText = "Alım tamamlandı fakat alıcı modu otomatik kapatılamadı.";
            });
        }
    }
    private async Task SaveHistoryIfNeededAsync(bool success, string direction, string status)
    {
        try
        {
            if (_historySavedForCurrentTransfer)
                return;

            _historySavedForCurrentTransfer = true;

            if (_historyService == null)
                return;

            var isSend = string.Equals(direction, "Gönderim", StringComparison.OrdinalIgnoreCase);

            var fileCount = _lastFileCount;
            var totalBytes = _lastTotalBytes;

            if (isSend)
            {
                if (fileCount <= 0)
                    fileCount = _allSelectedFiles.Count;

                if (totalBytes <= 0)
                    totalBytes = _allSelectedFiles.Sum(x => x.Size);
            }

            if (fileCount <= 0)
                fileCount = 1;

            if (totalBytes < 0)
                totalBytes = 0;

            var fileName = BuildHistoryFileNameSafe(isSend, fileCount);

            var deviceName = "Bu cihaz";

            try
            {
                if (!string.IsNullOrWhiteSpace(DeviceInfo.Current.Name))
                    deviceName = DeviceInfo.Current.Name;
            }
            catch
            {
                deviceName = "Bu cihaz";
            }

            var item = new TransferHistoryItem
            {
                FileName = fileName,
                FileCount = fileCount,
                TotalBytes = totalBytes,
                Direction = isSend ? "send" : "receive",
                IsSuccess = success,
                Message = string.IsNullOrWhiteSpace(status) ? "Transfer tamamlandı." : status,
                DeviceName = deviceName,
                CreatedAt = DateTime.Now
            };

            await _historyService.AddAsync(item);
        }
        catch
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusText = "Transfer tamamlandı fakat geçmiş kaydı oluşturulamadı.";
            });
        }
    }

    private string BuildHistoryFileNameSafe(bool isSend, int fileCount)
    {
        try
        {
            if (fileCount > 1)
                return $"{fileCount} dosya";

            if (isSend && SelectedFiles.Count == 1)
            {
                var selectedFileName = _allSelectedFiles[0]?.FileName;

                if (!string.IsNullOrWhiteSpace(selectedFileName))
                    return selectedFileName;
            }

            var currentName = CleanTransferFileNameSafe(TransferCurrentFileName);

            if (!string.IsNullOrWhiteSpace(currentName))
                return currentName;

            return isSend ? "Gönderilen dosya" : "Alınan dosya";
        }
        catch
        {
            return isSend ? "Gönderilen dosya" : "Alınan dosya";
        }
    }

    private static string CleanTransferFileNameSafe(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        var index = value.IndexOf(" - ", StringComparison.Ordinal);

        if (index >= 0 && index + 3 < value.Length)
            return value[(index + 3)..].Trim();

        return value.Trim();
    }
    private string BuildHistoryFileName(bool isSend, int fileCount)
    {
        if (fileCount > 1)
            return $"{fileCount} dosya";

        if (isSend && SelectedFiles.Count == 1)
            return SelectedFiles[0].FileName;

        var currentName = CleanTransferFileName(TransferCurrentFileName);

        if (!string.IsNullOrWhiteSpace(currentName))
            return currentName;

        return isSend ? "Gönderilen dosya" : "Alınan dosya";
    }

    private static string CleanTransferFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        var index = value.IndexOf(" - ", StringComparison.Ordinal);

        if (index >= 0 && index + 3 < value.Length)
            return value[(index + 3)..].Trim();

        return value.Trim();
    }
    private static bool IsSuccessStatus(string status)
    {
        return status.Contains("tamamlandı", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("başarıyla", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsErrorStatus(string status)
    {
        return status.Contains("hatası", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("başlatılamadı", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("bağlanılamadı", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024L * 1024L) return $"{bytes / 1024.0 / 1024.0:F1} MB";
        return $"{bytes / 1024.0 / 1024.0 / 1024.0:F1} GB";
    }
    private ImageSource CreateQrImage(RavenQrPayload payload)
    {
        var json = JsonSerializer.Serialize(payload);

        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(json, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(qrData);
        var bytes = qrCode.GetGraphic(20);

        return ImageSource.FromStream(() => new MemoryStream(bytes));
    }

    private void UpdateProgress(TransferProgress progress)
    {
        IsTransferPanelVisible = true;

        TransferProgress = progress.Percent / 100.0;
        TransferProgressText = $"{progress.Percent:F0}%";
        TransferSpeedText = progress.SpeedText;
        TransferRemainingText = progress.RemainingTimeText;
        TransferCurrentFileName =
            $"{progress.CurrentFileIndex}/{progress.TotalFiles} - {progress.CurrentFileName}";
    }

   

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}