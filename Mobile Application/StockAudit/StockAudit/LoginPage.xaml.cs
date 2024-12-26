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
    public partial class LoginPage : ContentPage
    {
        MasterLogic objMas = new MasterLogic();
        RequestProvider requestProvider = new RequestProvider();
        OrderService _orderservice;
        public LoginPage()
        {
            InitializeComponent();
           
            string Imei = "";

            Int64 ret = 0;
            objMas.FetchUniqueId(ref Imei);

            if (Imei != "" || Imei != null)
                Globals.IMEI = TabId.Text = Imei;
            else if (ret <= 0)
                objMas.ShowAlert("Error - [1]", objMas.DbErrBuf, this);
        }

        private async void btnSettings_Clicked(object sender, EventArgs e)
        {
            try
            {
                StackLayout frmLogn = this.FindByName<StackLayout>("popupLogin");
                await Task.Delay(150);

                if (frmLogn.IsVisible == true)
                    frmLogn.IsVisible = false;
                else
                    frmLogn.IsVisible = true;
            }
            catch (Exception Ex)
            {
                objMas.ShowAlert("Catche Error : C[B2]", Ex.Message.ToString(), this);
            }
        }

        int ButtonClick = 0;
        private async void btnLogin_Clicked(object sender, EventArgs e)
        {
            StackLayout stkLogin = this.FindByName<StackLayout>("popupLogin");
            try
            {
                int day, month, year;
                string sRunPwd = "";
                stkLogin.IsVisible = true;
                await Task.Delay(150);


                if (ButtonClick == 0)
                {
                    day = month = year = 0;
                    ButtonClick++;
                    if (txtPassword.Text == "")
                    {
                        objMas.ShowAlert("Message", "Input Valid Password !", this);
                        stkLogin.IsVisible = false;
                        ButtonClick = 0;
                        return;
                    }
                    string sDate = System.DateTime.Now.ToString("ddMMyy");
                    day = int.Parse(sDate.Substring(0, 2));
                    month = int.Parse(sDate.Substring(2, 2));
                    year = int.Parse(sDate.Substring(4, 2));

                    day = day + month;
                    year = year - 3;

                    sRunPwd = day.ToString().PadLeft(2, '0') + year.ToString().PadLeft(2, '0');

                    if (txtPassword.Text.Trim() != sRunPwd)
                    {
                        objMas.ShowAlert("Message", "Invalid Password", this);
                        ButtonClick = 0;
                        stkLogin.IsVisible = false;
                        txtPassword.Text = "";
                        return;
                    }

                    txtPassword.Text = "";
                    await Navigation.PushAsync(new FrmServerSettings(), true);
                    stkLogin.IsVisible = false;
                    ButtonClick = 0;
                }
            }
            catch (Exception Ex)
            {
                stkLogin.IsVisible = false;
                objMas.ShowAlert("Catche Error : C[B3]", Ex.Message.ToString(), this);
                ButtonClick = 0;
            }
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (ButtonClick == 0)
                {
                    ButtonClick++;
                    txtPassword.Text = "";
                    StackLayout stkLogin = this.FindByName<StackLayout>("popupLogin");
                    stkLogin.IsVisible = false;
                    ButtonClick = 0;
                }
            }
            catch (Exception Ex)
            {
                ButtonClick = 0;
                objMas.ShowAlert("Catche Error : C[B5]", Ex.Message.ToString(), this);

            }
        }

        private void txtMUserId_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtMPassword_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void btnStart_Clicked(object sender, EventArgs e)
        {
            StackLayout stkindicator = this.FindByName<StackLayout>("StkActIndicator");
           
            try
            {
                if (ButtonClick == 0)
                {
                    ButtonClick++;
                    stkindicator.IsVisible = true;
                    txtMUserId.Text = txtMUserId.Text.Trim();
                    txtMPassword.Text = txtMPassword.Text.Trim();

                    if (txtMUserId.Text == "" || txtMPassword.Text == "")
                    {
                        stkindicator.IsVisible = false;
                        objMas.ShowAlert("Message : ", "Input Valid User ID & Password !", this);
                        ButtonClick = 0;
                        return;
                    }
                    if (!Application.Current.Properties.ContainsKey("ServerUrl"))
                    {
                        stkindicator.IsVisible = false;
                        objMas.ShowAlert("Message : ", "Failed To Fetch Server Details", this);
                        ButtonClick = 0;
                        return;
                    }
                    Globals.ServerUrl = Application.Current.Properties["ServerUrl"].ToString();
                    _orderservice = new OrderService(requestProvider);
                    var usrdtls = await _orderservice.Login(Globals.Login, new Models.LoginModel { Imei = TabId.Text, UserId = txtMUserId.Text, Password=txtMPassword.Text });
                    if (usrdtls == null)
                    {
                        stkindicator.IsVisible = false;
                        objMas.ShowAlert("Message : ", "Data Fetch Failed", this);
                        ButtonClick = 0;
                        return;
                    }
                    if (usrdtls.Status != 1)
                    {
                        stkindicator.IsVisible = false;
                        objMas.ShowAlert("Message : ", usrdtls.Message, this);
                        ButtonClick = 0;
                        return;
                    }
                    txtMUserId.Text = txtMPassword.Text = "";
                    ButtonClick = 0;
                    stkindicator.IsVisible = false;
                    App.Current.MainPage = new NavigationPage(new MainPage());
                }
            }
            catch (Exception Ex)
            {
                ButtonClick = 0;
                stkindicator.IsVisible = false;
                objMas.ShowAlert("Catch Error : C[] ", Ex.Message.ToString(), this);
            }
        }
    }
}