using SalesReturn.Models;
using SalesReturn.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SalesReturn
{
    public partial class MainPage : ContentPage
    {
        MasterLogic objMas = new MasterLogic();
        RequestProvider requestProvider = new RequestProvider();
        OrderService _orderservice;
        PlantMaster plantDtls;
        public MainPage(PlantMaster plant)
        {
            InitializeComponent();
            try
            {
                plantDtls = plant;
                _orderservice = new OrderService(requestProvider);
                txtRackBarcode.Text = "OF01";
                txtRackBarcode.IsEnabled = false;
                LoadOutlet();
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }

        private async void LoadOutlet()
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try
            {
                stkindicator.IsVisible = true;
                var picklist = await _orderservice.GetOutlet(Globals.GetOutlet);
                if (picklist == null)
                {
                    objMas.ShowAlert("Message : ", "Date Fetch Failed", this);
                    stkindicator.IsVisible = false;
                    return;
                }
                if (picklist.Status != 1)
                {
                    objMas.ShowAlert("Message : ", picklist.Message, this);
                    stkindicator.IsVisible = false;
                    return;
                }
                var data = new List<OutletModel>();
                data.Add(new OutletModel { OutletName = "--Select--", OutletCode = "0" });
                var reqdata = picklist.data.Select(x => new OutletModel { OutletName = x.OutletName, OutletCode = x.OutletCode }).ToList();
                if (reqdata != null && reqdata.Count() >= 1)
                {
                    foreach (var kvp in reqdata)
                        data.Add(new OutletModel { OutletName = kvp.OutletName, OutletCode = kvp.OutletCode });
                }
                ddlOutlet.SelectedItem = null;
                ddlOutlet.DataSource = data;
                stkindicator.IsVisible = false;
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                Device.BeginInvokeOnMainThread(async () => await funFocus());
                return;
            }
        }
        private void btnScan_Clicked(object sender, EventArgs e)
        {
            try
            {   if (ddlOutlet.SelectedIndex <= 0)
                {
                    objMas.ShowAlert("Mesage", "Select Outlet", this);
                    return;
                }
                else if (txtTruckNo.Text == "") 
                {
                    objMas.ShowAlert("Message", "Input Truck No", this);
                    return;
                }
                btnScan.IsEnabled = txtMaterialBarcode.IsEnabled = txtTruckNo.IsEnabled = ddlOutlet.IsEnabled = false;
                txtCrateBarcode.IsEnabled = btnAccept.IsEnabled = btnHold.IsEnabled = true;
                Globals.CrateEnabled = true;
                Device.BeginInvokeOnMainThread(async () => await funFocus());
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        private async Task funFocus()
        {
            try
            {
                await Task.Delay(20);
                if (txtCrateBarcode.IsEnabled == true && txtCrateBarcode.IsFocused == false)
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
        private void txtRackBarcode_Completed(object sender, EventArgs e)
        {

        }
        private async void txtCrateBarcode_Completed(object sender, EventArgs e)
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try
            {
                if (txtCrateBarcode.Text == "")
                    return;

                stkindicator.IsVisible = true;
                var crtdtls = await _orderservice.ValidateCrate(Globals.GetCrateValidate, new BarcodeValidate { CrateBarcode = txtCrateBarcode.Text, PlantId = plantDtls.PlantId });
                if (crtdtls == null)
                {
                    txtCrateBarcode.Text = "";
                    objMas.ShowAlert("Message : ", "Data Fetch Failed", this);
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
                        RackBarcode = txtRackBarcode.Text,
                        PlantId = plantDtls.PlantId,
                        SecondaryBarcode = txtMaterialBarcode.Text,
                        AcceptorHold = btnAccept.IsChecked == true ? 1 : 2,
                        TruckNo = txtTruckNo.Text,
                        OutletCode = ddlOutlet.SelectedValue.ToString(),
                        OutletName = ddlOutlet.Text,
                        ScannedBy = "1"
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
                stkindicator.IsVisible = true;
                Navigation.PushAsync(new frmScannedDetails(plantDtls.PlantId));
                stkindicator.IsVisible = false;
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return;
            }
        }
        public async Task<int> Save()
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
            try
            {
                stkindicator.IsVisible = true;
                var crtdtls = await _orderservice.SaveSalesReturn(Globals.SaveSalesReturn, new BarcodeValidate { PlantId = plantDtls.PlantId , OutletCode = ddlOutlet.SelectedValue.ToString() });
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
                btnScan.IsEnabled = txtTruckNo.IsEnabled = ddlOutlet.IsEnabled = true;
                txtCrateBarcode.Text = txtTruckNo.Text = txtMaterialBarcode.Text = ddlOutlet.Text = "";
                txtCrateBarcode.IsEnabled = txtMaterialBarcode.IsEnabled = btnNextCrate.IsEnabled = false;
                objMas.ShowAlert("Message : ", crtdtls.Message, this);
                return 1;
            }
            catch (Exception Ex)
            {
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catche Error : ", Ex.Message.ToString(), this);
                return -1;
            }
        }
        private void btnSave_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (ddlOutlet.SelectedIndex <= 0)
                {
                    objMas.ShowAlert("Mesage", "Select Outlet", this);
                    return;
                }
               
                DependencyService.Get<ShowAlertWithFunction>().ShowAlertWithFunction("Confirm", "Are you sure to save....?", Save, this);
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
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
