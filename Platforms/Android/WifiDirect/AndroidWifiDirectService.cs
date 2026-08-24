using Android.Content;
using Android.Net.Wifi.P2p;
using Android.Util;
using RavenMobile.Features.Connection;
using AppContext = Android.App.Application;

namespace RavenMobile.Platforms.Android.WifiDirect;

public class AndroidWifiDirectService : BroadcastReceiver,
    IWifiDirectService,
    WifiP2pManager.IPeerListListener,
    WifiP2pManager.IConnectionInfoListener
{
    private readonly Context? _context;
    private readonly WifiP2pManager? _manager;
    private readonly WifiP2pManager.Channel? _channel;

    private bool _receiverRegistered;

    public event Action<WifiDirectPeer>? OnPeerFound;
    public event Action<WifiDirectPeer>? OnRavenDeviceFound;
    public event Action<string>? OnConnectionStatusChanged;
    public event Action<WifiDirectConnectionInfo>? ConnectionInfoAvailable;

    public AndroidWifiDirectService()
    {
        _context = AppContext.Context;

        if (_context == null)
        {
            Log.Debug("RAVEN_WIFI", "Context null");
            return;
        }

        _manager = _context.GetSystemService(Context.WifiP2pService) as WifiP2pManager;
        _channel = _manager?.Initialize(_context, _context.MainLooper, null);

        RegisterWifiDirectReceiver();

        Log.Debug("RAVEN_WIFI", "Wi-Fi Direct service hazır");
    }

    private void RegisterWifiDirectReceiver()
    {
        if (_context == null || _receiverRegistered)
            return;

        try
        {
            var filter = new IntentFilter();
            filter.AddAction(WifiP2pManager.WifiP2pStateChangedAction);
            filter.AddAction(WifiP2pManager.WifiP2pPeersChangedAction);
            filter.AddAction(WifiP2pManager.WifiP2pConnectionChangedAction);
            filter.AddAction(WifiP2pManager.WifiP2pThisDeviceChangedAction);

            _context.RegisterReceiver(this, filter);
            _receiverRegistered = true;

            Log.Debug("RAVEN_WIFI", "Wi-Fi Direct receiver kaydedildi");
        }
        catch (Exception ex)
        {
            Log.Debug("RAVEN_WIFI", $"Receiver kayıt hatası: {ex.Message}");
        }
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        var manager = _manager;
        var channel = _channel;

        if (intent?.Action == null || manager == null || channel == null)
            return;

        var action = intent.Action;

        if (action == WifiP2pManager.WifiP2pStateChangedAction)
        {
            var state = intent.GetIntExtra(WifiP2pManager.ExtraWifiState, -1);

            if (state == (int)WifiP2pState.Enabled)
                Log.Debug("RAVEN_WIFI", "Wi-Fi Direct açık");
            else
                OnConnectionStatusChanged?.Invoke("Wi-Fi Direct kapalı.");
        }
        else if (action == WifiP2pManager.WifiP2pPeersChangedAction)
        {
            try
            {
                manager.RequestPeers(channel, this);
            }
            catch (Exception ex)
            {
                Log.Debug("RAVEN_WIFI", $"Peer request hatası: {ex.Message}");
            }
        }
        else if (action == WifiP2pManager.WifiP2pConnectionChangedAction)
        {
            try
            {
                manager.RequestConnectionInfo(channel, this);
            }
            catch (Exception ex)
            {
                Log.Debug("RAVEN_WIFI", $"Connection info request hatası: {ex.Message}");
            }
        }
        else if (action == WifiP2pManager.WifiP2pThisDeviceChangedAction)
        {
            Log.Debug("RAVEN_WIFI", "Bu cihaz Wi-Fi Direct durumu değişti");
        }
    }

    public Task RegisterRavenServiceAsync()
    {
        return Task.CompletedTask;
    }

    public Task DiscoverRavenDevicesAsync()
    {
        return Task.CompletedTask;
    }

    public Task DiscoverPeersAsync()
    {
        var manager = _manager;
        var channel = _channel;

        if (manager == null || channel == null)
        {
            OnConnectionStatusChanged?.Invoke("Wi-Fi Direct hazır değil.");
            return Task.CompletedTask;
        }

        try
        {
            manager.DiscoverPeers(channel, new ActionListener(
                onSuccess: () =>
                {
                    Log.Debug("RAVEN_WIFI", "Peer discovery başladı");

                    try
                    {
                        manager.RequestPeers(channel, this);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("RAVEN_WIFI", $"RequestPeers hatası: {ex.Message}");
                    }
                },
                onFailure: reason =>
                {
                    Log.Debug("RAVEN_WIFI", $"Peer discovery hata: {reason}");
                }));
        }
        catch (Exception ex)
        {
            Log.Debug("RAVEN_WIFI", $"DiscoverPeers exception: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public async Task<bool> StartConnectionAsync(string deviceAddress)
    {
        var manager = _manager;
        var channel = _channel;

        if (manager == null || channel == null)
        {
            OnConnectionStatusChanged?.Invoke("Wi-Fi Direct hazır değil.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(deviceAddress))
        {
            OnConnectionStatusChanged?.Invoke("Cihaz adresi boş.");
            return false;
        }

        await StopPeerDiscoverySafeAsync();
        await CancelConnectSafeAsync();

        await Task.Delay(900);

        var intents = new[] { 7, 0, 15 };

        for (int i = 0; i < intents.Length; i++)
        {
            var attempt = i + 1;
            var intent = intents[i];

            OnConnectionStatusChanged?.Invoke($"Wi-Fi Direct bağlantısı deneniyor... ({attempt}/3)");

            var success = await TryConnectOnceAsync(deviceAddress, intent);

            if (success)
            {
                OnConnectionStatusChanged?.Invoke("Wi-Fi Direct bağlantı isteği gönderildi. Bağlantı bilgisi bekleniyor...");

                _ = Task.Run(async () =>
                {
                    await Task.Delay(2500);
                    await RequestConnectionInfoAsync();

                    await Task.Delay(2500);
                    await RequestConnectionInfoAsync();

                    await Task.Delay(3000);
                    await RequestConnectionInfoAsync();
                });

                return true;
            }

            await CancelConnectSafeAsync();
            await StopPeerDiscoverySafeAsync();

            await Task.Delay(1200);
        }

        OnConnectionStatusChanged?.Invoke("Wi-Fi Direct bağlantı hatası: hedef cihaz hazır değil.");
        return false;
    }

    private Task<bool> TryConnectOnceAsync(string deviceAddress, int groupOwnerIntent)
    {
        var manager = _manager;
        var channel = _channel;

        var tcs = new TaskCompletionSource<bool>();

        if (manager == null || channel == null)
        {
            tcs.TrySetResult(false);
            return tcs.Task;
        }

        try
        {
            var config = new WifiP2pConfig
            {
                DeviceAddress = deviceAddress,
                GroupOwnerIntent = groupOwnerIntent
            };

            manager.Connect(channel, config, new ActionListener(
                onSuccess: () =>
                {
                    Log.Debug("RAVEN_WIFI", $"Connect success. GO Intent={groupOwnerIntent}");
                    tcs.TrySetResult(true);
                },
                onFailure: reason =>
                {
                    Log.Debug("RAVEN_WIFI", $"Connect failure. Reason={reason}, GO Intent={groupOwnerIntent}");
                    OnConnectionStatusChanged?.Invoke($"Wi-Fi Direct deneme başarısız: {reason}");
                    tcs.TrySetResult(false);
                }));
        }
        catch (Exception ex)
        {
            Log.Debug("RAVEN_WIFI", $"Connect exception: {ex.Message}");
            OnConnectionStatusChanged?.Invoke($"Wi-Fi Direct connect exception: {ex.Message}");
            tcs.TrySetResult(false);
        }

        return tcs.Task;
    }

    public async Task ResetConnectionAsync()
    {
        OnConnectionStatusChanged?.Invoke("Eski bağlantı temizleniyor...");

        await StopPeerDiscoverySafeAsync();
        await CancelConnectSafeAsync();
        await ClearServiceRequestsSafeAsync();
        await ClearLocalServicesSafeAsync();
        await RemoveGroupSafeAsync();

        await Task.Delay(1500);

        OnConnectionStatusChanged?.Invoke("Bağlantı temizlendi.");
    }

    public async Task DisconnectAsync()
    {
        OnConnectionStatusChanged?.Invoke("Bağlantı kapatılıyor...");

        await StopPeerDiscoverySafeAsync();
        await CancelConnectSafeAsync();
        await RemoveGroupSafeAsync();

        await Task.Delay(1000);

        OnConnectionStatusChanged?.Invoke("Bağlantı kapatıldı.");
    }

    public Task RequestConnectionInfoAsync()
    {
        var manager = _manager;
        var channel = _channel;

        if (manager == null || channel == null)
            return Task.CompletedTask;

        try
        {
            manager.RequestConnectionInfo(channel, this);
        }
        catch (Exception ex)
        {
            Log.Debug("RAVEN_WIFI", $"RequestConnectionInfo hatası: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public void OnPeersAvailable(WifiP2pDeviceList? peers)
    {
        if (peers == null)
            return;

        foreach (var device in peers.DeviceList)
        {
            var name = device.DeviceName;

            if (string.IsNullOrWhiteSpace(name))
                name = "Wi-Fi Direct Device";

            var address = device.DeviceAddress ?? "";

            if (string.IsNullOrWhiteSpace(address))
                continue;

            var status = (int)device.Status;

            Log.Debug("RAVEN_WIFI", $"Peer bulundu: {name} - {address} - Status={status}");

            OnPeerFound?.Invoke(new WifiDirectPeer
            {
                Name = name,
                Address = address,
                Status = status
            });
        }
    }

    public void OnConnectionInfoAvailable(WifiP2pInfo? info)
    {
        if (info == null)
        {
            OnConnectionStatusChanged?.Invoke("Bağlantı bilgisi alınamadı.");
            return;
        }

        if (!info.GroupFormed)
        {
            Log.Debug("RAVEN_WIFI", "Group henüz oluşmadı");
            return;
        }

        var ownerAddress = info.GroupOwnerAddress?.HostAddress ?? "";

        Log.Debug(
            "RAVEN_WIFI",
            $"ConnectionInfo: IsGroupOwner={info.IsGroupOwner}, OwnerAddress={ownerAddress}");

        ConnectionInfoAvailable?.Invoke(new WifiDirectConnectionInfo
        {
            IsConnected = true,
            IsGroupOwner = info.IsGroupOwner,
            GroupOwnerAddress = ownerAddress
        });

        OnConnectionStatusChanged?.Invoke(
            info.IsGroupOwner
                ? "Wi-Fi Direct bağlantısı kuruldu. Bu cihaz Group Owner."
                : $"Wi-Fi Direct bağlantısı kuruldu. Group Owner IP: {ownerAddress}");
    }

    private Task StopPeerDiscoverySafeAsync()
    {
        var manager = _manager;
        var channel = _channel;

        var tcs = new TaskCompletionSource<bool>();

        if (manager == null || channel == null)
        {
            tcs.TrySetResult(false);
            return tcs.Task;
        }

        try
        {
            manager.StopPeerDiscovery(channel, new ActionListener(
                onSuccess: () => tcs.TrySetResult(true),
                onFailure: reason => tcs.TrySetResult(false)));
        }
        catch
        {
            tcs.TrySetResult(false);
        }

        return tcs.Task;
    }

    private Task CancelConnectSafeAsync()
    {
        var manager = _manager;
        var channel = _channel;

        var tcs = new TaskCompletionSource<bool>();

        if (manager == null || channel == null)
        {
            tcs.TrySetResult(false);
            return tcs.Task;
        }

        try
        {
            manager.CancelConnect(channel, new ActionListener(
                onSuccess: () => tcs.TrySetResult(true),
                onFailure: reason => tcs.TrySetResult(false)));
        }
        catch
        {
            tcs.TrySetResult(false);
        }

        return tcs.Task;
    }

    private Task RemoveGroupSafeAsync()
    {
        var manager = _manager;
        var channel = _channel;

        var tcs = new TaskCompletionSource<bool>();

        if (manager == null || channel == null)
        {
            tcs.TrySetResult(false);
            return tcs.Task;
        }

        try
        {
            manager.RemoveGroup(channel, new ActionListener(
                onSuccess: () => tcs.TrySetResult(true),
                onFailure: reason => tcs.TrySetResult(false)));
        }
        catch
        {
            tcs.TrySetResult(false);
        }

        return tcs.Task;
    }

    private Task ClearServiceRequestsSafeAsync()
    {
        var manager = _manager;
        var channel = _channel;

        var tcs = new TaskCompletionSource<bool>();

        if (manager == null || channel == null)
        {
            tcs.TrySetResult(false);
            return tcs.Task;
        }

        try
        {
            manager.ClearServiceRequests(channel, new ActionListener(
                onSuccess: () => tcs.TrySetResult(true),
                onFailure: reason => tcs.TrySetResult(false)));
        }
        catch
        {
            tcs.TrySetResult(false);
        }

        return tcs.Task;
    }

    private Task ClearLocalServicesSafeAsync()
    {
        var manager = _manager;
        var channel = _channel;

        var tcs = new TaskCompletionSource<bool>();

        if (manager == null || channel == null)
        {
            tcs.TrySetResult(false);
            return tcs.Task;
        }

        try
        {
            manager.ClearLocalServices(channel, new ActionListener(
                onSuccess: () => tcs.TrySetResult(true),
                onFailure: reason => tcs.TrySetResult(false)));
        }
        catch
        {
            tcs.TrySetResult(false);
        }

        return tcs.Task;
    }

    private class ActionListener : Java.Lang.Object, WifiP2pManager.IActionListener
    {
        private readonly Action? _onSuccess;
        private readonly Action<WifiP2pFailureReason>? _onFailure;

        public ActionListener(
            Action? onSuccess = null,
            Action<WifiP2pFailureReason>? onFailure = null)
        {
            _onSuccess = onSuccess;
            _onFailure = onFailure;
        }

        public void OnSuccess()
        {
            _onSuccess?.Invoke();
        }

        public void OnFailure(WifiP2pFailureReason reason)
        {
            _onFailure?.Invoke(reason);
        }
    }
}