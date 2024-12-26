using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StockAudit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FrmServerSettings : ContentPage
    {
        MasterLogic objMas = new MasterLogic();
        List<string> ProductionPlantCode = new List<string>();
        List<string> CFAPlantCode = new List<string>();
        public FrmServerSettings()
        {
            InitializeComponent();
            try
            {
                if (Application.Current.Properties.ContainsKey("ServerUrl"))
                    txtServer.Text = Application.Current.Properties["ServerUrl"].ToString();
                else
                    txtServer.Text = "";
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error : A[] Page Load ", Ex.Message.ToString(), this);
            }
        }

        protected async void btnSrvStgSave_Clicked(object sender, EventArgs e)
        {
            try
            {
                txtServer.Text = txtServer.Text.Trim();
                if (txtServer.Text == "")
                {
                    objMas.ShowAlert("Message : ", "Input Server URL !", this);
                    return;
                }
                Application.Current.Properties["ServerUrl"]= txtServer.Text;
                await Application.Current.SavePropertiesAsync();
                objMas.ShowAlert("Message :", "Saved Successfully !", this);
                await Task.Delay(1000);
                Navigation.RemovePage(this);
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catch Error : C[] ", Ex.Message.ToString(), this);
            }
        }
    }
}
