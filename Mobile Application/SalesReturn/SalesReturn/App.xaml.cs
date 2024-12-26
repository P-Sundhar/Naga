using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SalesReturn
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //20.1.0.47
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjU1MzY3QDMyMzAyZTMxMmUzMGRSODE1eXRPaTdtYVRTb1FCZG54WW9iUm04b2w2cUhjUXVvSXVTR0dTN1k9");
            MainPage = new NavigationPage(new LoginPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
