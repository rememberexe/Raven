package crc6421e67ae3323efb58;


public class AndroidWifiJoinService_WifiNetworkCallback
	extends android.net.ConnectivityManager.NetworkCallback
	implements
		mono.android.IGCUserPeer
{

	public AndroidWifiJoinService_WifiNetworkCallback ()
	{
		super ();
		if (getClass () == AndroidWifiJoinService_WifiNetworkCallback.class) {
			mono.android.TypeManager.Activate ("RavenMobile.Platforms.Android.WifiQr.AndroidWifiJoinService+WifiNetworkCallback, RavenMobile", "", this, new java.lang.Object[] {  });
		}
	}

	public void onAvailable (android.net.Network p0)
	{
		n_onAvailable (p0);
	}

	private native void n_onAvailable (android.net.Network p0);

	public void onUnavailable ()
	{
		n_onUnavailable ();
	}

	private native void n_onUnavailable ();

	public void onLost (android.net.Network p0)
	{
		n_onLost (p0);
	}

	private native void n_onLost (android.net.Network p0);

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
