using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(SalesReturn.Droid.PageEntryRenderer))]
namespace SalesReturn.Droid
{
#pragma warning disable CS0618 // Type or member is obsolete
    internal class PageEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            Control?.SetBackgroundColor(Android.Graphics.Color.Transparent);
            Control.SetSelectAllOnFocus(true);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}