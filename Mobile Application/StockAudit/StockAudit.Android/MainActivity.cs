using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using AndroidX.Core.Content;
using Android.Views;
using Xamarin.Forms.Platform.Android;
using Android;

namespace StockAudit.Droid
{
    [Activity(Label = "StockAudit", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (PermissionChecker.CheckSelfPermission(this, Manifest.Permission.ReadPhoneState) == (int)(Android.Content.PM.Permission.Denied))
                RequestPermissions(new string[] { Manifest.Permission.ReadPhoneState }, 0);

            if (PermissionChecker.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == (int)(Android.Content.PM.Permission.Denied))
                RequestPermissions(new String[] { Manifest.Permission.ReadExternalStorage }, 0);

            if (PermissionChecker.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)(Android.Content.PM.Permission.Denied))
                RequestPermissions(new String[] { Manifest.Permission.WriteExternalStorage }, 0);

            if (PermissionChecker.CheckSelfPermission(this, Manifest.Permission.AccessNetworkState) == (int)(Android.Content.PM.Permission.Denied))
                RequestPermissions(new String[] { Manifest.Permission.AccessNetworkState }, 0);

            if (PermissionChecker.CheckSelfPermission(this, Manifest.Permission.AccessWifiState) == (int)(Android.Content.PM.Permission.Denied))
                RequestPermissions(new String[] { Manifest.Permission.AccessWifiState }, 0);

            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            this.Window.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TurnScreenOn);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var stBarHeight = typeof(FormsAppCompatActivity).GetField("statusBarHeight", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (stBarHeight == null)
                {
                    stBarHeight = typeof(FormsAppCompatActivity).GetField("_statusBarHeight", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                }
                stBarHeight?.SetValue(this, 0);
            }

            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}