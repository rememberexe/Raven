#if ANDROID

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Util;
using RavenMobile.Features.WifiQr;
using RavenMobile.Features.WifiQr.Models;
using AppContext = Android.App.Application;

namespace RavenMobile.Platforms.Android.WifiQr;

public class AndroidHotspotService : IHotspotService
{
    private const int Port = 50555;

    private WifiManager.LocalOnlyHotspotReservation? _reservation;

    public event Action<string>? OnStatusChanged;

    public Task<HotspotSessionInfo> StartHotspotAsync()
    {
        var tcs = new TaskCompletionSource<HotspotSessionInfo>();

        try
        {
            var context = AppContext.Context;

            if (context == null)
            {
                tcs.TrySetException(new Exception("Android context alınamadı."));
                return tcs.Task;
            }

            var wifiManager = context.GetSystemService(Context.WifiService) as WifiManager;

            if (wifiManager == null)
            {
                tcs.TrySetException(new Exception("WifiManager alınamadı."));
                return tcs.Task;
            }

            OnStatusChanged?.Invoke("Yerel Wi-Fi ağı oluşturuluyor...");

            var callback = new LocalHotspotCallback(
                onStarted: reservation =>
                {
                    try
                    {
                        _reservation = reservation;

                        var config = reservation.WifiConfiguration;

                        var ssid = config?.Ssid?.Trim('"') ?? "";
                        var password = config?.PreSharedKey?.Trim('"') ?? "";

                        if (string.IsNullOrWhiteSpace(ssid))
                            throw new Exception("Hotspot SSID alınamadı.");

                        if (string.IsNullOrWhiteSpace(password))
                            throw new Exception("Hotspot şifresi alınamadı.");

                        var hostIp = FindLikelyHotspotIp();

                        Log.Debug("RAVEN_HOTSPOT", $"SSID={ssid}, IP={hostIp}");

                        OnStatusChanged?.Invoke("Alıcı ağı hazır. QR kodu okutabilirsin.");

                        tcs.TrySetResult(new HotspotSessionInfo
                        {
                            Ssid = ssid,
                            Password = password,
                            HostAddress = hostIp,
                            Port = Port
                        });
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                },
                onStopped: () =>
                {
                    OnStatusChanged?.Invoke("Yerel Wi-Fi ağı durdu.");
                },
                onFailed: reason =>
                {
                    OnStatusChanged?.Invoke($"Hotspot başlatılamadı: {reason}");
                    tcs.TrySetException(new Exception($"Hotspot başlatılamadı: {reason}"));
                });

            wifiManager.StartLocalOnlyHotspot(callback, new Handler(Looper.MainLooper!));
        }
        catch (Exception ex)
        {
            OnStatusChanged?.Invoke($"Hotspot hatası: {ex.Message}");
            tcs.TrySetException(ex);
        }

        return tcs.Task;
    }

    public Task StopHotspotAsync()
    {
        try
        {
            _reservation?.Close();
            _reservation = null;

            OnStatusChanged?.Invoke("Alıcı ağı kapatıldı.");
        }
        catch (Exception ex)
        {
            OnStatusChanged?.Invoke($"Hotspot kapatma hatası: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static string FindLikelyHotspotIp()
    {
        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;

                var properties = networkInterface.GetIPProperties();

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    var ip = address.Address.ToString();

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    if (ip.StartsWith("192.168.") ||
                        ip.StartsWith("172.") ||
                        ip.StartsWith("10."))
                    {
                        return ip;
                    }
                }
            }
        }
        catch
        {
        }

        return "192.168.43.1";
    }

    private class LocalHotspotCallback : WifiManager.LocalOnlyHotspotCallback
    {
        private readonly Action<WifiManager.LocalOnlyHotspotReservation> _onStarted;
        private readonly Action _onStopped;
        private readonly Action<LocalOnlyHotspotCallbackErrorCode> _onFailed;

        public LocalHotspotCallback(
            Action<WifiManager.LocalOnlyHotspotReservation> onStarted,
            Action onStopped,
            Action<LocalOnlyHotspotCallbackErrorCode> onFailed)
        {
            _onStarted = onStarted;
            _onStopped = onStopped;
            _onFailed = onFailed;
        }

        public override void OnStarted(WifiManager.LocalOnlyHotspotReservation reservation)
        {
            _onStarted.Invoke(reservation);
        }

        public override void OnStopped()
        {
            _onStopped.Invoke();
        }

        public override void OnFailed(LocalOnlyHotspotCallbackErrorCode reason)
        {
            _onFailed.Invoke(reason);
        }
    }
}

#endif