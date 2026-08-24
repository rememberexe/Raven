package crc6477b0797571f03227;


public class FrameAnalyzer
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.camera.core.ImageAnalysis.Analyzer
{

	public FrameAnalyzer ()
	{
		super ();
		if (getClass () == FrameAnalyzer.class) {
			mono.android.TypeManager.Activate ("ZXing.Net.Maui.FrameAnalyzer, ZXing.Net.MAUI", "", this, new java.lang.Object[] {  });
		}
	}

	public android.util.Size getDefaultTargetResolution ()
	{
		return n_getDefaultTargetResolution ();
	}

	private native android.util.Size n_getDefaultTargetResolution ();

	public int getTargetCoordinateSystem ()
	{
		return n_getTargetCoordinateSystem ();
	}

	private native int n_getTargetCoordinateSystem ();

	public void analyze (androidx.camera.core.ImageProxy p0)
	{
		n_analyze (p0);
	}

	private native void n_analyze (androidx.camera.core.ImageProxy p0);

	public void updateTransform (android.graphics.Matrix p0)
	{
		n_updateTransform (p0);
	}

	private native void n_updateTransform (android.graphics.Matrix p0);

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
