using StockAudit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StockAudit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class frmScannedDetails : ContentPage
    {
        MasterLogic objMas = new MasterLogic();
        RequestProvider requestProvider = new RequestProvider();
        OrderService _orderservice;
        public frmScannedDetails(string Plant)
        {
            InitializeComponent(); 
            _orderservice = new OrderService(requestProvider);
            LoadGrid(Plant);
        }

        private async void LoadGrid(string Plant)
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try
            {
                stkindicator.IsVisible = true;
                var plantdtls = await _orderservice.GetScannedDetails(Globals.GetStockDetails, new Models.BarcodeValidate {PlantId = Plant });
                if (plantdtls == null)
                {
                    objMas.ShowAlert("Message : ", "Date Fetch Failed", this);
                    stkindicator.IsVisible = false;
                    return;
                }
                if (plantdtls.Status != 1)
                {
                    objMas.ShowAlert("Message : ", plantdtls.Message, this);
                    stkindicator.IsVisible = false;
                    return;
                }
                gdvScannedDetails.ItemsSource = plantdtls.data;
                stkindicator.IsVisible = false;
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }

        private void btnBack_Clicked(object sender, EventArgs e)
        {
            Navigation.RemovePage(this);
        }
    }
}