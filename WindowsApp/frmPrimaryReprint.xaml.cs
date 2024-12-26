using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Naga
{
    /// <summary>
    /// Interaction logic for frmPrimaryReprint.xaml
    /// </summary>
    public partial class frmPrimaryReprint : Window
    {
        MasterLogic objMas = new MasterLogic();
        UsbPrint objUsbpnt = new UsbPrint();
        string PdnId = "", MatCode = "", Batch = "";
        public frmPrimaryReprint(string MaterialCode,string PDNId,string BatchNo)
        {
            InitializeComponent();
            PdnId = PDNId;
            MatCode = MaterialCode;
            Batch = BatchNo;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (txtFrom.Text == "" || txtFrom.Text == "0")
            {
                MessageBox.Show("Input Valid From & To For Reprint");
                return;
            }
            //if (Convert.ToInt32(txtTo.Text) >= Convert.ToInt32(txtFrom.Text) || Convert.ToInt32(txtFrom.Text) == Convert.ToInt32(txtTo.Text)) 
            //{
            //    MessageBox.Show("Input Valid From & To For Reprint");
            //    return;
            //}
            Int64 ret = objMas.GetTableCount("primarybarcode where prdcnt >= " + txtFrom.Text + " and PDNId='" + PdnId + "' and MaterialCode='" +
                MatCode + "' and Barcode='" + Batch + "'");
            if (ret <= 0)
            {
                MessageBox.Show("Data Not Available For Reprint");
                return;
            }
            for (int i = 1; i <= Convert.ToInt32(txtFrom.Text); i++)
            {
                PrintLabel(Batch);
                objMas.ExecQry("Update primarybarcode set ReworkCnt=ReworkCnt + 1 where PDNId='" + PdnId + "' and MaterialCode='" + MatCode + "' and Barcode='" + Batch + "'");
            }
        }

        private void PrintLabel(string BatchNo)
        {
            try
            {
                string buf = "";

                if (File.Exists(ConfigurationManager.AppSettings["PRNFILE2"]) == true)
                    buf = System.IO.File.ReadAllText(ConfigurationManager.AppSettings["PRNFILE2"]);
                else
                {
                    MessageBox.Show("PRN File 1 Not Found", buf);
                    return;
                }

                buf = buf.Replace("STRTEXT1", BatchNo);
                objUsbpnt.SendStringToPrinter(Globals.PRINTER_NAME2, buf);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed To Print Barcode Label" + ex.Message.ToString());
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
