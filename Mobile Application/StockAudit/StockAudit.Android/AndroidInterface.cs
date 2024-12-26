using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Net.Wifi;
using Android.Views.InputMethods;
using Android.Widget;
using StockAudit.Droid;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

[assembly: Dependency(typeof(AndroidInterface))]
namespace StockAudit.Droid
{
    class AndroidInterface : GetDeviceID, IKeyboardHelper, ICloseApplication, Toast, DBPath, ShowAlertWithFunction
    {
        public string dbpath()
        {
            return Android.OS.Environment.ExternalStorageDirectory.ToString();
        }
        public void GetUniqueId(ref string Result)
        {
            try
            {
                Result = "";
                Context con = Android.App.Application.Context;
                Android.Telephony.TelephonyManager tm = (Android.Telephony.TelephonyManager)con.GetSystemService(Context.TelephonyService);

                int MobileVersion = Convert.ToInt16(Android.OS.Build.VERSION.Sdk);

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
                    Result = tm.GetDeviceId(0);
                else
                    Result = tm.DeviceId;

                if (Result == "" || Result == null)//Android 10
                    Result = Android.Provider.Settings.Secure.GetString(con.ContentResolver, Android.Provider.Settings.Secure.AndroidId);

                if (Result == "" || Result == null)
                {
                    WifiManager manager = (WifiManager)con.GetSystemService(Context.WifiService);
                    WifiInfo info = manager.ConnectionInfo;
                    Result = info.MacAddress;
                    if (Result == "02:00:00:00:00:00")
                    {
                        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                        foreach (var nif in interfaces)
                        {
                            if (!nif.Name.ToLower().Contains("wlan")) continue;

                            var physicalAddress = nif.GetPhysicalAddress();
                            byte[] macBytes = physicalAddress.GetAddressBytes();

                            string macString = BitConverter.ToString(macBytes);
                            if (!string.IsNullOrWhiteSpace(macString))
                                Result = macString.Trim().ToUpper().Replace("-", ":");
                        }
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Result = ex.Message.ToString();
                return;
            }
        }
        public void GetDevId(ref string Result)
        {
            try
            {
                Result = "";
                Context con = Android.App.Application.Context;
                Android.Telephony.TelephonyManager tm = (Android.Telephony.TelephonyManager)con.GetSystemService(Context.TelephonyService);

                int MobileVersion = Convert.ToInt16(Android.OS.Build.VERSION.Sdk);

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
                    Result = tm.GetDeviceId(0);
                else
                    Result = tm.DeviceId;

                return;
            }
            catch (Exception ex)
            {
                Result = ex.Message.ToString();
                return;
            }
        }
        public void HideKeyboard()
                    {
            try
            {
                var context = Forms.Context;
                var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;
                if (inputMethodManager != null && context is Activity)
                {
                    var activity = context as Activity;
                    var token = activity.CurrentFocus?.WindowToken;
                    inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
                    activity.Window.DecorView.ClearFocus();
                }
            }
            catch (Exception Ex)
            {

            }
        }
        public void Show(string message)
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }
        public void closeApplication()
        {
            var activity = (Activity)Forms.Context;
            activity.FinishAffinity();
        }
        public void ShowAlert(string title, string Msg, Xamarin.Forms.ContentPage forms)
        {
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(Xamarin.Forms.Forms.Context);
            builder.SetIcon(Android.Resource.Drawable.IcDialogInfo);
            //builder.SetIcon(Android.Resource.Drawable.IcDialogAlert);
            builder.SetTitle(title);
            builder.SetMessage(Msg);
            builder.SetCancelable(false);
            builder.SetPositiveButton("OK", (senderAlert, args) =>
            {
            });
            builder.Show();

        }
        public void ShowAlertWithFunction(string title, string Msg, Func<Task<int>> function, Xamarin.Forms.ContentPage cntpg)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(Xamarin.Forms.Forms.Context);
            builder.SetIcon(Android.Resource.Drawable.IcDialogInfo);
            //builder.SetIcon(Android.Resource.Drawable.IcDialogAlert);
            builder.SetTitle(title);
            builder.SetMessage(Msg);
            builder.SetCancelable(false);
            builder.SetPositiveButton("Yes", (senderAlert, args) =>
            {

                var progressDialog = Android.App.ProgressDialog.Show(Xamarin.Forms.Forms.Context, "Please wait", "Loading...", true);
                progressDialog.Indeterminate = false;
                progressDialog.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
                new Thread(new ThreadStart(delegate
                {
                    Thread.Sleep(500);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        function();
                        progressDialog.Hide();
                    });
                })).Start();

            });
            builder.SetNegativeButton("No", (senderAlert, args) =>
            {
            });
            builder.Show();

#pragma warning restore CS0618 // Type or member is obsolete
        }

    }
}