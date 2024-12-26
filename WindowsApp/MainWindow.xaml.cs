using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Naga
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            int ret = 0;
            string buf = "0.490";

            pnlLicence.Visibility = Visibility.Hidden;
            EncDecFun objE = new EncDecFun();
            ret = objE.GetSystemId(ref buf);
            if (ret != 1)
            {
                MessageBox.Show("Run this Application as Administrator \n or Try Reinstalling Application", "Error - A[1a]");
                Application.Current.Shutdown();
                return;
            }
            lblMachineId.Text = buf;
            Globals.Device = ConvertHex(buf);
            buf = "";
            ret = objE.ChkLicence(ref buf);
            if (ret == 0)
            {
                MessageBox.Show("Run this Application as Administrator \n or Try Reinstalling Application" + objE.sErrBuf, "Error - A[17]");
                Application.Current.Shutdown();
                return;
            }
            else if (ret <= 0)
            {
                MessageBox.Show("Run this Application as Administrator \n or Try Reinstalling Application", "Error - A[18]");
                Application.Current.Shutdown();
                return;
            }
            else if (ret == 100)
            {
                txtMacId.Text = buf;
                txtLicence.Text = "";
                pnlLicence.Visibility = Visibility.Visible;
                txtLicence.Focus();
                return;
            }
            else if (ret != 1)
            {
                MessageBox.Show("Run this Application as Administrator \n or Try Reinstalling Application", "Error - A[19]");
                Application.Current.Shutdown();
                return;
            }
            int Ret = LoadSettings();
            if (Ret <= 0)
            {
                MessageBox.Show("Add settings Text Document");
                return;
            }
            if (File.Exists(ConfigurationManager.AppSettings["PRNFILE1"]) == false)
            {
                MessageBox.Show("Add Prn File Document");
                Application.Current.Shutdown();
            }

            CloseAll();
            txtUserId.Focus();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MasterLogic objMas = new MasterLogic();
                Int64 Ret = 0;
                string buf = "";
                DataTable dt = new DataTable();
               
                if (txtUserId.Text.Trim() == "" || txtPassword.Password == "")
                {
                    MessageBox.Show("Enter Valid UserId And Password ?");
                    return;
                }
                Ret = objMas.ChkDBConnection();
                if (Ret <= 0)
                {
                    MessageBox.Show("DataBase Connection Failed!");
                    return;
                }
                if (txtUserId.Text == "Admin" && txtPassword.Password == "12345")
                    Application.Current.Shutdown();
                dt = objMas.ValidateLogin(txtUserId.Text, txtPassword.Password.ToString());
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Input Valid UserId");
                    return;
                }
                if (dt.Rows.Count > 0)
                {

                    DataTable dbdt = objMas.GetDataTable(" Select EmpDesc,b.UnitId,b.PlantId from employee_master a,systemuser_master b Where a.EmpId=b.EmpId " +
                         " AND b.Status in (1,2) AND b.UserId='" + txtUserId.Text.Trim() + "'");
                    if (dbdt == null)
                    {
                        MessageBox.Show("Error" + objMas.DBErrBuf);
                        return;
                    }
                    else if (Globals.Plant != dbdt.Rows[0]["PlantId"].ToString())
                    {
                        MessageBox.Show("Invalid User For This Plant");
                        return;
                    }
                    else if (Globals.Unit != dbdt.Rows[0]["UnitId"].ToString())
                    {
                        MessageBox.Show("Invalid User For This Unit");
                        return;
                    }
                    Globals.UserHeader = dbdt.Rows[0]["EmpDesc"].ToString();
                    Globals.User = txtUserId.Text.Trim();
                    Settings objSetting = new Settings();
                    objSetting.ShowDialog();
                    this.Close();
                }
                else
                    MessageBox.Show("Login Failed.Try Again");
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message.ToString());
            }
        }

        public void CloseAll()
        {
            Window objWin = Window.GetWindow(this);
            foreach (Window openWin in System.Windows.Application.Current.Windows)
            {
                if (openWin != objWin)
                    openWin.Close();
            }
        }

        private int LoadSettings()
        {
            try 
            {
                string buf = "";
                string[] value = new string[2];
                int i = 0;
                if (File.Exists(Globals.SettingPath) == false)
                    return 0;
                using (StreamReader sr = new StreamReader(Globals.SettingPath))
                {
                    while (true)
                    {
                        buf = "";
                        buf = sr.ReadLine();
                        if (buf == "")
                            break;
                        if (buf.Contains(':') == true)
                        {
                            value = buf.Split(':');
                            if (i == 0)
                                Globals.System = value[1].ToString();
                            else if (i == 1)
                                Globals.Database = value[1].ToString();
                            else if (i == 2)
                                Globals.IP = value[1].ToString();
                            else if (i == 3)
                                Globals.Plant = value[1].ToString();
                            else if (i == 4)
                                Globals.Unit = value[1].ToString();
                            i++;
                        }
                    }
                }
                return 1;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private void btnLicence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int ret = 0;

                if (txtLicence.Text == "")
                {
                    MessageBox.Show("Input valid Licence", "Caution - A[20]");
                    return;
                }
                else if (txtLicence.Text.Length != 9)
                {
                    MessageBox.Show("Input valid Licence", "Caution - A[21]");
                    return;
                }

                EncDecFun objE = new EncDecFun();

                ret = objE.ChkInputtedLicence(txtLicence.Text);
                if (ret == 0)
                {
                    MessageBox.Show("Run this Application as Administrator \n or Try Reinstalling Application\n" + objE.sErrBuf, "Error A[22]");
                    return;
                }
                else if (ret <= 0)
                {
                    MessageBox.Show("Run this Application as Administrator \n or Try Reinstalling Application\n" + objE.sErrBuf, "Error A[23]");
                    return;
                }
                else if (ret == 100)
                {
                    MessageBox.Show("Input Valid Licence" + "\n" + "Contact System Administrator");
                    txtLicence.Text = "";
                    txtLicence.Focus();
                    return;
                }
                else if (ret != 1)
                {
                    MessageBox.Show("Run this Application as Administrator \n or Try Reinstalling Application", "Error [1, 3]");
                    return;
                }
                pnlLicence.Visibility = Visibility.Hidden;
            }

            catch (Exception ex)
            {
                MessageBox.Show("C[1,5]: Error");
            }
        }

        private string HostService()
        {
            string buf = "";
            try
            {
                WebService Service = new WebService(Globals.SERVICE_URL, Globals.SERVICE_METHOD);
                string[] DataKey = { "ApplnId", "ClientId", "Device", "ApplnVer" };
                string[] DataValue = { Globals.APPLNID, Globals.CLIENTID, Globals.System + "," + Globals.Device, Globals.APPLN_VERSION };

                Service.Invoke(DataKey.Length, DataKey, DataValue);
                buf = Service.ResultString;
                return buf;
            }
            catch (Exception Ex)
            {
                buf = Ex.Message.ToString();
                MessageBox.Show(buf);
                return buf;
            }
        }

        public static string ConvertHex(String hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    String hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;

                }

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }

        public void HitWebService()
        {
            try 
            {
                Int64 Ret = 0;
                MasterLogic objMas = new MasterLogic();

                Ret = objMas.GetTableCount("systemdetails where Device='" + Globals.Device + "'");
                if (Ret == 0)
                {
                    Ret = objMas.ExecQry(" Insert into systemdetails(Device,LastUsedOn,UnitId,System)Values('" + Globals.Device + "',Curdate(),'" + Globals.Unit
                                         + "','" + Globals.System + "')");
                    HostService();
                }
                else if (Ret >= 1)
                {
                    Ret = objMas.GetTableCount("systemdetails Where Date(LastUsedOn)=CurDate() AND Device='" + Globals.Device + "'");
                    if (Ret == 0)
                    {
                        Ret = objMas.ExecQry("Update systemdetails Set LastUsedOn=CurDate() Where Device='" + Globals.Device + "'");
                        HostService();
                    }
                }
            }
            catch (Exception)
            {
 
            }
        }

    }
}
