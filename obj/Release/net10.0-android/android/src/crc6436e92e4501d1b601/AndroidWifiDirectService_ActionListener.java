package crc6436e92e4501d1b601;


public class AndroidWifiDirectService_ActionListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.net.wifi.p2p.WifiP2pManager.ActionListener
{

	public AndroidWifiDirectService_ActionListener ()
	{
		super ();
		if (getClass () == AndroidWifiDirectService_ActionListener.class) {
			mono.android.TypeManager.Activate ("RavenMobile.Platforms.Android.WifiDirect.AndroidWifiDirectService+ActionListener, RavenMobile", "", this, new java.lang.Object[] {  });
		}
	}

	public void onFailure (int p0)
	{
		n_onFailure (p0);
	}

	private native void n_onFailure (int p0);

	public void onSuccess ()
	{
		n_onSuccess ();
	}

	private native void n_onSuccess ();

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
