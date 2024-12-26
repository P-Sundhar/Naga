using StockAudit.Models;
using StockAudit.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StockAudit
{
    public partial class MainPage : ContentPage
    {
        MasterLogic objMas = new MasterLogic();
        RequestProvider requestProvider = new RequestProvider();
        OrderService _orderservice;
        public MainPage()
        {
            InitializeComponent(); 
            _orderservice = new OrderService(requestProvider); 
            LoadWarehouse();
        }

        private async void LoadWarehouse()
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try
            {
                stkindicator.IsVisible = true;
                var plantdtls = await _orderservice.GetPlantDetails(Globals.GetPlant);
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
                ddlWarehouse.DataSource = plantdtls.data;
                stkindicator.IsVisible = false;
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private void btnScan_Clicked(object sender, EventArgs e)
        {
            try 
            {
                if (ddlWarehouse.Text == "")
                {
                    objMas.ShowAlert("Message", "Select WareHouse", this);
                    return;
                }
                btnScan.IsEnabled = ddlWarehouse.IsEnabled =  txtMaterialBarcode.IsEnabled = false;
                txtRackBarcode.IsEnabled = false;
                txtRackBarcode.Text = "OF01";
                txtCrateBarcode.IsEnabled = true;
                Globals.CrateEnabled = true;
                Device.BeginInvokeOnMainThread(async () => await funFocus());
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private void txtRackBarcode_Completed(object sender, EventArgs e)
        {
            try
            {
                if (txtRackBarcode.Text == "")
                    return;
                txtRackBarcode.IsEnabled = false;
                txtCrateBarcode.IsEnabled = true;
                Globals.CrateEnabled = true;
                Device.BeginInvokeOnMainThread(async () => await funFocus());
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private async void txtCrateBarcode_Completed(object sender, EventArgs e)
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try
            {
                if (txtCrateBarcode.Text == "")
                    return;

                stkindicator.IsVisible = true;
                var crtdtls = await _orderservice.ValidateCrate(Globals.GetCrateValidate, new BarcodeValidate { CrateBarcode = txtCrateBarcode.Text });
                if (crtdtls == null)
                {
                    txtCrateBarcode.Text = "";
                    objMas.ShowAlert("Message : ", "Date Fetch Failed", this);
                    stkindicator.IsVisible = false;
                    return;
                }
                if (crtdtls.Status != 1)
                {
                    txtCrateBarcode.Text = "";
                    objMas.ShowAlert("Message : ", crtdtls.Message, this);
                    stkindicator.IsVisible = false;
                    return;
                }
                stkindicator.IsVisible = false;
                txtCrateBarcode.IsEnabled = Globals.CrateEnabled = false;
                Globals.SecondaryEnabled = true;
                txtMaterialBarcode.IsEnabled = btnNextCrate.IsEnabled = true;
                Device.BeginInvokeOnMainThread(async () => await funFocus());
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private async Task funFocus()
        {
            try
            {
                await Task.Delay(20);
                if (txtRackBarcode.IsEnabled == true && txtRackBarcode.IsFocused == false)
                {
                    txtRackBarcode.Text = "";
                    txtRackBarcode.Focus();
                    await Task.Delay(50);
                    objMas.HideKeyBoard();
                }
                else if (txtCrateBarcode.IsEnabled == true && txtCrateBarcode.IsFocused == false)
                {
                    txtCrateBarcode.Text = "";
                    txtCrateBarcode.Focus();
                    await Task.Delay(50);
                    objMas.HideKeyBoard();
                }
                else if (txtMaterialBarcode.IsEnabled == true && txtMaterialBarcode.IsFocused == false)
                {
                    txtMaterialBarcode.Text = "";
                    txtMaterialBarcode.Focus();
                    await Task.Delay(50);
                    objMas.HideKeyBoard();
                }
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private void ddlWarehouse_SelectionChanged(object sender, Syncfusion.XForms.ComboBox.SelectionChangedEventArgs e)
        {

        }
        private async void txtMaterialBarcode_Completed(object sender, EventArgs e)
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try
            {
                if (txtMaterialBarcode.Text == "")
                    return;

                stkindicator.IsVisible = true;
                var crtdtls = await _orderservice.ValidateBarcode(Globals.GetBarcodeValidate,
                    new BarcodeValidate
                    {
                        CrateBarcode = txtCrateBarcode.Text,
                        RackBarcode=txtRackBarcode.Text,
                        PlantId=ddlWarehouse.SelectedValue.ToString(),
                        SecondaryBarcode=txtMaterialBarcode.Text,
                        ScannedBy="1"
                    });
                if (crtdtls == null)
                {
                    txtMaterialBarcode.Text = "";
                    objMas.ShowAlert("Message : ", "Date Fetch Failed", this);
                    stkindicator.IsVisible = false;
                    return;
                }
                if (crtdtls.Status != 1)
                {
                    txtMaterialBarcode.Text = "";
                    objMas.ShowAlert("Message : ", crtdtls.Message, this);
                    stkindicator.IsVisible = false;
                    return;
                }
                stkindicator.IsVisible = false;
                btnNextCrate.IsEnabled = true;
                txtMaterialBarcode.Text = "";
                Device.BeginInvokeOnMainThread(async () => await funFocus());
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private void btnNextCrate_Clicked(object sender, EventArgs e)
        {
            try 
            {
                txtCrateBarcode.Text = "";
                txtMaterialBarcode.IsEnabled = false;
                txtCrateBarcode.IsEnabled = true;
                Globals.SecondaryEnabled = false;
                Globals.CrateEnabled = true;
                Device.BeginInvokeOnMainThread(async () => await funFocus());
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private void btnView_Clicked(object sender, EventArgs e)
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try 
            {
                if (ddlWarehouse.Text == "")
                {
                    objMas.ShowAlert("Message", "Select WareHouse", this);
                    return;
                }
                stkindicator.IsVisible = true;
                Navigation.PushAsync(new frmScannedDetails(ddlWarehouse.SelectedValue.ToString()));
                stkindicator.IsVisible = false;
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private void btnExit_Clicked(object sender, EventArgs e)
        {
            try 
            {
                DependencyService.Get<ShowAlertWithFunction>().ShowAlertWithFunction("Confirm", "Are you sure to exit....?", AppExit, this);
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error", Ex.Message.ToString(), this);
            }
        }
        public async Task<int> AppExit()
        {
            try
            {
                DependencyService.Get<ICloseApplication>().closeApplication();
                return 1;
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error", Ex.Message.ToString(), this);
                return -1;
            }
        }
        private void btnSave_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (ddlWarehouse.Text == "")
                {
                    objMas.ShowAlert("Message", "Select WareHouse", this);
                    return;
                }
                DependencyService.Get<ShowAlertWithFunction>().ShowAlertWithFunction("Confirm", "Are you sure to save....?", Save, this);
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error", Ex.Message.ToString(), this);
            }
        }
        public async Task<int> Save()
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try
            {
                stkindicator.IsVisible = true;
                var crtdtls = await _orderservice.SaveStockAudit(Globals.SaveStockAudit, new BarcodeValidate { PlantId = ddlWarehouse.SelectedValue.ToString() });
                if (crtdtls == null)
                {
                    stkindicator.IsVisible = false;
                    objMas.ShowAlert("Message : ", "Data Fetch Failed", this);
                    return -1;
                }
                if (crtdtls.Status != 1)
                {
                    stkindicator.IsVisible = false;
                    objMas.ShowAlert("Message : ", crtdtls.Message, this);
                    return 1;
                }
                
                stkindicator.IsVisible = false;
                ddlWarehouse.IsEnabled = btnScan.IsEnabled = true;
                ddlWarehouse.Text = txtCrateBarcode.Text = txtRackBarcode.Text = "";
                txtCrateBarcode.IsEnabled = txtMaterialBarcode.IsEnabled = btnNextCrate.IsEnabled = false;
                return 1;
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return -1;
            }
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
