package crc6436e92e4501d1b601;


public class AndroidWifiDirectService
	extends android.content.BroadcastReceiver
	implements
		mono.android.IGCUserPeer,
		android.net.wifi.p2p.WifiP2pManager.PeerListListener,
		android.net.wifi.p2p.WifiP2pManager.ConnectionInfoListener
{

	public AndroidWifiDirectService ()
	{
		super ();
		if (getClass () == AndroidWifiDirectService.class) {
			mono.android.TypeManager.Activate ("RavenMobile.Platforms.Android.WifiDirect.AndroidWifiDirectService, RavenMobile", "", this, new java.lang.Object[] {  });
		}
	}

	public void onReceive (android.content.Context p0, android.content.Intent p1)
	{
		n_onReceive (p0, p1);
	}

	private native void n_onReceive (android.content.Context p0, android.content.Intent p1);

	public void onPeersAvailable (android.net.wifi.p2p.WifiP2pDeviceList p0)
	{
		n_onPeersAvailable (p0);
	}

	private native void n_onPeersAvailable (android.net.wifi.p2p.WifiP2pDeviceList p0);

	public void onConnectionInfoAvailable (android.net.wifi.p2p.WifiP2pInfo p0)
	{
		n_onConnectionInfoAvailable (p0);
	}

	private native void n_onConnectionInfoAvailable (android.net.wifi.p2p.WifiP2pInfo p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
