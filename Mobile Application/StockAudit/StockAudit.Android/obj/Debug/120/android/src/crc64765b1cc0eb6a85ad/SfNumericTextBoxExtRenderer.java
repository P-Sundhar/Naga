package crc64765b1cc0eb6a85ad;


public class SfNumericTextBoxExtRenderer
	extends crc649bfc1964ebae3690.SfNumericTextBoxRenderer
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_dispatchKeyEventPreIme:(Landroid/view/KeyEvent;)Z:GetDispatchKeyEventPreIme_Landroid_view_KeyEvent_Handler\n" +
			"";
		mono.android.Runtime.register ("Syncfusion.SfDataGrid.XForms.Droid.SfNumericTextBoxExtRenderer, Syncfusion.SfDataGrid.XForms.Android", SfNumericTextBoxExtRenderer.class, __md_methods);
	}


	public SfNumericTextBoxExtRenderer (android.content.Context p0)
	{
		super (p0);
		if (getClass () == SfNumericTextBoxExtRenderer.class) {
			mono.android.TypeManager.Activate ("Syncfusion.SfDataGrid.XForms.Droid.SfNumericTextBoxExtRenderer, Syncfusion.SfDataGrid.XForms.Android", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
		}
	}


	public SfNumericTextBoxExtRenderer (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == SfNumericTextBoxExtRenderer.class) {
			mono.android.TypeManager.Activate ("Syncfusion.SfDataGrid.XForms.Droid.SfNumericTextBoxExtRenderer, Syncfusion.SfDataGrid.XForms.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
		}
	}


	public SfNumericTextBoxExtRenderer (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == SfNumericTextBoxExtRenderer.class) {
			mono.android.TypeManager.Activate ("Syncfusion.SfDataGrid.XForms.Droid.SfNumericTextBoxExtRenderer, Syncfusion.SfDataGrid.XForms.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
		}
	}


	public boolean dispatchKeyEventPreIme (android.view.KeyEvent p0)
	{
		return n_dispatchKeyEventPreIme (p0);
	}

	private native boolean n_dispatchKeyEventPreIme (android.view.KeyEvent p0);

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
