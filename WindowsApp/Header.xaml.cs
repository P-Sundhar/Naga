using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
            Title.Text = ConfigurationManager.AppSettings["Title"].ToString();
            txtUser.Text = " Welcome : " + Globals.UserHeader;
        }

        private void btnshtdwn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Globals.StartEndFlag == 1)
                {
                    MessageBox.Show("First Stop The Process ..");
                    return;
                }
                MessageBoxResult Ret = new MessageBoxResult();
                Ret = MessageBox.Show("   Are You Sure to Shutdown ?", "Confirm", MessageBoxButton.YesNo);
                if (Ret == MessageBoxResult.Yes)
                    Process.Start("shutdown", "/s /t 0");
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[] " + Ex.Message.ToString());
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Globals.StartEndFlag == 1)
                {
                    MessageBox.Show("First Stop The Process ..");
                    return;
                }
                MessageBoxResult Ret = new MessageBoxResult();
                Ret = MessageBox.Show("   Are You Sure to Logout ?", "Confirm", MessageBoxButton.YesNo);
                if (Ret == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                    System.Diagnostics.Process.Start("Naga.exe");
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[] " + Ex.Message.ToString());
            }
        }

        private void mnuProduction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Package frm = new Package();
                frm.ShowDialog();
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[4,7]" + Ex.Message.ToString());
            }
        }

        private void mnuPrimaryBarcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frmPrimary frm = new frmPrimary();
                frm.ShowDialog();
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[4,7]" + Ex.Message.ToString());
            }
        }
    }
}
