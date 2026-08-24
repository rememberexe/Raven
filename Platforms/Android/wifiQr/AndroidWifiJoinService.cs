#if ANDROID

using System.Net.Sockets;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Util;
using RavenMobile.Features.WifiQr;
using RavenMobile.Features.WifiQr.Models;
using AppContext = Android.App.Application;

namespace RavenMobile.Platforms.Android.WifiQr;

public class AndroidWifiJoinService : IWifiJoinService
{
    private ConnectivityManager? _connectivityManager;
    private WifiNetworkCallback? _networkCallback;
    private Network? _currentNetwork;

    public event Action<string>? OnStatusChanged;

    public Task<WifiJoinResult> ConnectAsync(string ssid, string password)
    {
        var tcs = new TaskCompletionSource<WifiJoinResult>();

        try
        {
            var context = AppContext.Context;

            if (context == null)
            {
                tcs.TrySetException(new Exception("Android context alınamadı."));
                return tcs.Task;
            }

            _connectivityManager =
                context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;

            if (_connectivityManager == null)
            {
                tcs.TrySetException(new Exception("ConnectivityManager alınamadı."));
                return tcs.Task;
            }

            DisconnectAsync();

            OnStatusChanged?.Invoke("Alıcı Wi-Fi ağına bağlanılıyor...");

            var specifier = new WifiNetworkSpecifier.Builder()
                .SetSsid(ssid)
                .SetWpa2Passphrase(password)
                .Build();

            var request = new NetworkRequest.Builder()
                .AddTransportType(TransportType.Wifi)
                .RemoveCapability(NetCapability.Internet)
                .SetNetworkSpecifier(specifier)
                .Build();

            _networkCallback = new WifiNetworkCallback(
                onAvailable: async network =>
                {
                    try
                    {
                        _currentNetwork = network;

                        try
                        {
                            _connectivityManager.BindProcessToNetwork(network);
                        }
                        catch (Exception ex)
                        {
                            OnStatusChanged?.Invoke($"Ağ yönlendirme uyarısı: {ex.Message}");
                        }

                        await Task.Delay(800);

                        var result = BuildJoinResult(ssid, network);

                        OnStatusChanged?.Invoke(
                            string.IsNullOrWhiteSpace(result.GatewayAddress)
                                ? "Wi-Fi ağına bağlandı."
                                : $"Wi-Fi ağına bağlandı. Alıcı IP: {result.GatewayAddress}");

                        tcs.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                },
                onUnavailable: () =>
                {
                    OnStatusChanged?.Invoke("Wi-Fi ağına bağlanılamadı.");
                    tcs.TrySetResult(new WifiJoinResult
                    {
                        IsConnected = false,
                        Ssid = ssid
                    });
                },
                onLost: network =>
                {
                    OnStatusChanged?.Invoke("Wi-Fi bağlantısı kesildi.");
                });

            _connectivityManager.RequestNetwork(request, _networkCallback);

            _ = Task.Run(async () =>
            {
                await Task.Delay(35000);

                if (!tcs.Task.IsCompleted)
                {
                    OnStatusChanged?.Invoke("Wi-Fi bağlantısı zaman aşımına uğradı.");

                    tcs.TrySetResult(new WifiJoinResult
                    {
                        IsConnected = false,
                        Ssid = ssid
                    });
                }
            });
        }
        catch (Exception ex)
        {
            OnStatusChanged?.Invoke($"Wi-Fi bağlantı hatası: {ex.Message}");
            tcs.TrySetException(ex);
        }

        return tcs.Task;
    }

    public Task DisconnectAsync()
    {
        try
        {
            _connectivityManager?.BindProcessToNetwork(null);

            if (_networkCallback != null)
            {
                try
                {
                    _connectivityManager?.UnregisterNetworkCallback(_networkCallback);
                }
                catch
                {
                }
            }

            _networkCallback = null;
            _currentNetwork = null;
        }
        catch
        {
        }

        return Task.CompletedTask;
    }

    private WifiJoinResult BuildJoinResult(string ssid, Network network)
    {
        var result = new WifiJoinResult
        {
            IsConnected = true,
            Ssid = ssid
        };

        try
        {
            var linkProperties = _connectivityManager?.GetLinkProperties(network);

            if (linkProperties != null)
            {
                foreach (var route in linkProperties.Routes)
                {
                    var gateway = route.Gateway?.HostAddress;

                    if (!string.IsNullOrWhiteSpace(gateway) &&
                        gateway != "0.0.0.0" &&
                        gateway.Contains('.') &&
                        !gateway.Contains(':'))
                    {
                        result.GatewayAddress = gateway;
                        break;
                    }
                }

                foreach (var address in linkProperties.LinkAddresses)
                {
                    var hostAddress = address.Address?.HostAddress;

                    if (!string.IsNullOrWhiteSpace(hostAddress) &&
                        hostAddress.Contains('.') &&
                        !hostAddress.Contains(':'))
                    {
                        result.LocalAddress = hostAddress;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug("RAVEN_WIFI_JOIN", $"Gateway alma hatası: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(result.GatewayAddress))
            result.GatewayAddress = "192.168.43.1";

        return result;
    }

    private class WifiNetworkCallback : ConnectivityManager.NetworkCallback
    {
        private readonly Action<Network> _onAvailable;
        private readonly Action _onUnavailable;
        private readonly Action<Network> _onLost;

        public WifiNetworkCallback(
            Action<Network> onAvailable,
            Action onUnavailable,
            Action<Network> onLost)
        {
            _onAvailable = onAvailable;
            _onUnavailable = onUnavailable;
            _onLost = onLost;
        }

        public override void OnAvailable(Network network)
        {
            _onAvailable.Invoke(network);
        }

        public override void OnUnavailable()
        {
            _onUnavailable.Invoke();
        }

        public override void OnLost(Network network)
        {
            _onLost.Invoke(network);
        }
    }
}

#endif