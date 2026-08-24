package crc64438e99c69d36dbd9;


public class AndroidBluetoothDiscoveryService
	extends android.content.BroadcastReceiver
	implements
		mono.android.IGCUserPeer
{

	public AndroidBluetoothDiscoveryService ()
	{
		super ();
		if (getClass () == AndroidBluetoothDiscoveryService.class) {
			mono.android.TypeManager.Activate ("RavenMobile.Platforms.Android.Bluetooth.AndroidBluetoothDiscoveryService, RavenMobile", "", this, new java.lang.Object[] {  });
		}
	}

	public void onReceive (android.content.Context p0, android.content.Intent p1)
	{
		n_onReceive (p0, p1);
	}

	private native void n_onReceive (android.content.Context p0, android.content.Intent p1);

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
