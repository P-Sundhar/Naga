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
    /// Interaction logic for frmPrimary.xaml
    /// </summary>
    public partial class frmPrimary : Window
    {
        MasterLogic objMas = new MasterLogic();
        UsbPrint objUsbpnt = new UsbPrint();
        public frmPrimary()
        {
            InitializeComponent();
            LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable(" Select " + Globals.DDL_INT_SELECT + " AS MaterialCode, '" + Globals.DDL_STR_SELECT
                        + "' AS MaterialDesc,0 as Denominator,0 as TargetQty,0 as PrimaryLabelCnt, 0 as PrimaryCnt,'' as BatchNo,0 as PDNID UNION SELECT Distinct a.MaterialCode,b.MaterialDesc," +
                        "b.Denominator,a.TargetQty,a.PrimaryQty as PrimaryLabelCnt,ifnull(c.PrdCnt, 0) as PrimaryCnt,a.BatchNo,a.PDNID FROM " +
                        "material_master b, production_header a left join primarybarcode c on(a.PDNId = c.PDNID and a.MaterialCode = c.MaterialCode) " +
                        "where a.JobFlag = 0 and Date(a.PDNDate) = CurDate() and a.MaterialCode = b.MaterialCode Order by MaterialDesc");

                if (dt == null)
                {
                    MessageBox.Show("Error - A[2,1] " + objMas.DBErrBuf);
                    return;
                }
                cbItems.DataContext = dt;
                cbItems.DisplayMemberPath = "MaterialDesc";
                cbItems.SelectedValuePath = "MaterialCode";
                cbItems.SelectedIndex = 0;
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[2,1]" + Ex.Message.ToString());
            }
        }

        private void cbItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                txtStartCnt.Text = txtEndCnt.Text = txtBatchNo.Text = "";
                if (cbItems.SelectedIndex == 0)
                    return;

                var data = cbItems.SelectedItem as DataRowView;
                if (data != null)
                {
                    txtBatchNo.Text = data.Row["BatchNo"].ToString();
                    txtStartCnt.Text = Convert.ToInt32(data.Row["PrimaryCnt"]) + 1 + "";
                    txtEndCnt.Text = data.Row["PrimaryLabelCnt"].ToString();
                    return;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[2,1]" + Ex.Message.ToString());
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                string PDNId = "";

                

                if (cbItems.SelectedIndex == 0)
                {
                    MessageBox.Show("Select Items To Print Primary Barcode");
                    return;
                }
                var data = cbItems.SelectedItem as DataRowView;
                if (data == null)
                {
                    MessageBox.Show("Failed To Fetch Production ID");
                    return;
                }
                else
                    PDNId = data.Row["PDNID"].ToString();

                for (int i = Convert.ToInt32(txtStartCnt.Text); i <= Convert.ToInt32(txtEndCnt.Text); i++)
                {
                    PrintLabel(txtBatchNo.Text);
                    objMas.ExecQry("Insert into primarybarcode(PDNId,MaterialCode,Barcode,PrdCnt,ReworkCnt,ExcessCnt)values('" + PDNId + "','" + cbItems.SelectedValue
                        + "','" + txtBatchNo.Text + "',1,0,0) on duplicate key Update PrdCnt=PrdCnt+1 ");
                }
                txtStartCnt.Text = txtEndCnt.Text = txtBatchNo.Text = "";
                LoadItems();
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Catche Error : ", Ex.Message.ToString());
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

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbItems.SelectedIndex == 0)
                {
                    MessageBox.Show("Select Items To Reprint Primary Barcode");
                    return;
                }
                var data = cbItems.SelectedItem as DataRowView;
                if (data == null)
                {
                    MessageBox.Show("Failed To Fetch Production ID");
                    return;
                }
                frmPrimaryReprint MsgBox = new frmPrimaryReprint(cbItems.SelectedValue.ToString(), data.Row["PDNID"].ToString(), txtBatchNo.Text);
                MsgBox.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed To Print Barcode Label" + ex.Message.ToString());
            }
        }
    }
}
