using RavenMobile.Features.Discovery.Model;

namespace RavenMobile.Features.Discovery;

public interface IBluetoothDiscoveryService
{
    void StartDiscovery();
    void StopDiscovery();

    event Action<DiscoveredDevice> OnDeviceFound;
}