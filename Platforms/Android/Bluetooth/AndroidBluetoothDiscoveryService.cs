using Android.Bluetooth;
using Android.Content;
using Android.Util;
using RavenMobile.Features.Discovery;
using RavenMobile.Features.Discovery.Model;
using AppContext = Android.App.Application;

namespace RavenMobile.Platforms.Android.Bluetooth;

public class AndroidBluetoothDiscoveryService : BroadcastReceiver, IBluetoothDiscoveryService
{
    public event Action<DiscoveredDevice>? OnDeviceFound;

    public AndroidBluetoothDiscoveryService()
    {
    }

    private (BluetoothAdapter? Adapter, Context? Context) GetSystem()
    {
        var context = AppContext.Context;

        if (context == null)
        {
            Log.Debug("RAVEN", "Context NULL");
            return (null, null);
        }

        var manager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var adapter = manager?.Adapter;

        return (adapter, context);
    }

    public void StartDiscovery()
    {
        var (adapter, context) = GetSystem();

        if (adapter == null || context == null)
        {
            Log.Debug("RAVEN", "Adapter veya context yok");
            return;
        }

        if (!adapter.IsEnabled)
        {
            Log.Debug("RAVEN", "Bluetooth kapalı");
            return;
        }

        if (adapter.IsDiscovering)
            adapter.CancelDiscovery();

        try
        {
            context.UnregisterReceiver(this);
        }
        catch
        {
        }

        var filter = new IntentFilter();
        filter.AddAction(BluetoothDevice.ActionFound);

        context.RegisterReceiver(this, filter);

        var started = adapter.StartDiscovery();

        Log.Debug("RAVEN", "Discovery başladı: " + started);
    }

    public void StopDiscovery()
    {
        var (adapter, context) = GetSystem();

        if (adapter == null || context == null)
            return;

        if (adapter.IsDiscovering)
            adapter.CancelDiscovery();

        try
        {
            context.UnregisterReceiver(this);
        }
        catch
        {
        }
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent?.Action != BluetoothDevice.ActionFound)
            return;

        var device = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) as BluetoothDevice;

        if (device == null)
            return;

        var name = device.Name;

        if (string.IsNullOrWhiteSpace(name))
            name = "Unknown Device";

        OnDeviceFound?.Invoke(new DiscoveredDevice
        {
            Name = name,
            Address = device.Address ?? "",
            LastSeen = DateTime.Now,
            DeviceType = GetDeviceType(device)
        });
    }

    private RavenDeviceType GetDeviceType(BluetoothDevice device)
    {
        var majorClass = device.BluetoothClass?.MajorDeviceClass;

        return majorClass switch
        {
            MajorDeviceClass.Phone => RavenDeviceType.Phone,
            MajorDeviceClass.Computer => RavenDeviceType.Computer,
            MajorDeviceClass.AudioVideo => RavenDeviceType.Headset,
            MajorDeviceClass.Wearable => RavenDeviceType.Watch,
            MajorDeviceClass.Imaging => RavenDeviceType.Computer,
            _ => RavenDeviceType.Unknown
        };
    }
}