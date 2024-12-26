using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Reprint : Window
    {
        UsbPrint objUsbpnt = new UsbPrint();
        MasterLogic objMas = new MasterLogic();
        string ItemShtName1 = "";
        string ItemShtName2 = "";

        public Reprint()
        {
            InitializeComponent();
            txtBarcode.Focus();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                int Ret = 0;
                Int64 ItemCount = 0;
                if (txtBarcode.Text == "")
                {
                    MessageBox.Show("Input Barcode Value");
                    return;
                }
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("Select a.materialCode,a.Weight,a.BatchNo,a.MonthYear,c.MaterialDesc,c.NetWgt,Date_Format(b.ExpDate,'%d-%b-%Y') As ExpDate," +
                    " Date_Format(b.PackDate,'%d-%b-%Y') As PackDate,b.UnitId,c.ItemShortName1,c.ItemShortName2,a.BoxNo from Production_lines a,production_header b,material_master c " +
                    " Where a.Barcode='" + txtBarcode.Text + "' AND a.PDNId = b.PDNId AND a.System=b.System AND a.materialCode=b.materialCode " +
                    " AND a.materialCode = c.materialCode");
                if (dt == null)
                {
                    MessageBox.Show("Error : Failed To Print BarCode" + objMas.DBErrBuf);
                    return;
                }
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No Details Found");
                    return;
                }
                else 
                {
                    ItemShtName1 = ItemShtName2 = "";
                    if (dt.Rows[0]["ItemShortName1"].ToString() != "")
                        ItemShtName1 = dt.Rows[0]["ItemShortName1"].ToString();

                    if (dt.Rows[0]["ItemShortName2"].ToString() != "")
                        ItemShtName2 = dt.Rows[0]["ItemShortName2"].ToString();

                    if (dt.Rows[0]["BoxNo"].ToString() != "")
                        ItemCount = Convert.ToInt64(dt.Rows[0]["BoxNo"]);

                    Ret = PrintLabel(dt.Rows[0]["materialCode"].ToString(), dt.Rows[0]["Weight"].ToString(), dt.Rows[0]["BatchNo"].ToString(),
                        dt.Rows[0]["MaterialDesc"].ToString(), txtBarcode.Text, dt.Rows[0]["MonthYear"].ToString(), "",
                        dt.Rows[0]["ExpDate"].ToString(), dt.Rows[0]["PackDate"].ToString(), ItemCount, dt.Rows[0]["UnitId"].ToString(), dt.Rows[0]["NetWgt"].ToString());
                    if (Ret == 1)
                    {
                        Ret = objMas.ExecQry("Insert Into Reprint_master(Barcode,TranDate,BarcodeCnt)Values('" + txtBarcode.Text + "',Now(),1) On duplicate Key Update BarcodeCnt=BarcodeCnt+1");
                        if (Ret <= 0)
                        {
                            MessageBox.Show("Barcode Count Update Failed");
                            return;
                        }
                        this.Hide();
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Failed To Print Barcode Label" + Ex.Message.ToString());
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private int PrintLabel(string ItemCode, string Weight, string BatchNo, string ItemName, string RunningNo, string Month, string MRP,string ExpDate,
            string PKDDate,Int64 ItemCount,string Unit, string NetWgt)
        {
            try
            {
                int Ret = 0;
                string buf = "";
                string Barcode = RunningNo;
                string[] Mnth = new string[2];
                Mnth = Month.Split(',');

                if (Globals.OTA_TESTING == true)
                    return 1;

                    if (File.Exists(ConfigurationManager.AppSettings["PRNFILE1"]) == true)
                        buf = System.IO.File.ReadAllText(ConfigurationManager.AppSettings["PRNFILE1"]);
                    else
                    {
                        MessageBox.Show("PRN File 1 Not Found", buf);
                        return 0;
                    }
                

                buf = buf.Replace("STRTEXT1", ItemShtName1);
                buf = buf.Replace("STRTEXT2", ItemShtName2);
                buf = buf.Replace("STRBATCHNO", BatchNo);
                buf = buf.Replace("STRUNIT", Unit);
                buf = buf.Replace("STRNETWGT", NetWgt);
                buf = buf.Replace("STRACTWGT", Weight);
                buf = buf.Replace("STRPKDDATE", PKDDate);
                buf = buf.Replace("STREXPDATE", ExpDate);
                buf = buf.Replace("STRMRP", MRP);
                buf = buf.Replace("STRBOXNO", ItemCount.ToString());
                buf = buf.Replace("STRBARCODE", Barcode);
                buf = buf.Replace("STRMONTH", Mnth[0] + " - " + Mnth[1]);

                Ret = objUsbpnt.SendStringToPrinter(Globals.PRINTER_NAME1, buf);
                return Ret;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed To Print Barcode Label" + ex.Message.ToString());
                return 0;
            }
        }
    }
}
