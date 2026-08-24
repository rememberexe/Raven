package crc6421e67ae3323efb58;


public class AndroidHotspotService_LocalHotspotCallback
	extends android.net.wifi.WifiManager.LocalOnlyHotspotCallback
	implements
		mono.android.IGCUserPeer
{

	public AndroidHotspotService_LocalHotspotCallback ()
	{
		super ();
		if (getClass () == AndroidHotspotService_LocalHotspotCallback.class) {
			mono.android.TypeManager.Activate ("RavenMobile.Platforms.Android.WifiQr.AndroidHotspotService+LocalHotspotCallback, RavenMobile", "", this, new java.lang.Object[] {  });
		}
	}

	public void onStarted (android.net.wifi.WifiManager.LocalOnlyHotspotReservation p0)
	{
		n_onStarted (p0);
	}

	private native void n_onStarted (android.net.wifi.WifiManager.LocalOnlyHotspotReservation p0);

	public void onStopped ()
	{
		n_onStopped ();
	}

	private native void n_onStopped ();

	public void onFailed (int p0)
	{
		n_onFailed (p0);
	}

	private native void n_onFailed (int p0);

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
