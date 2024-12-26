using System;
using System.Collections.Generic;
using System.Data;
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
using System.IO.Ports;
using System.Windows.Threading;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Timers;
using System.Globalization;
using System.Net.Sockets;
using System.Net;

namespace Naga
{
    /// <summary>
    /// Interaction logic for Package.xaml
    /// </summary>
    public partial class Package : Window
    {
        public delegate void dlProc1DataRecv(string Text);
        public dlProc1DataRecv dfnProc1Update;

        public delegate void dlProcComClose();
        public dlProcComClose dfnProcComClose;

        MasterLogic objMas = new MasterLogic();
        UsbPrint objUsbpnt = new UsbPrint();
        ComClass objCom = new ComClass();
        SerialPort serial = new SerialPort();

        public string ItemShtName1 = "";
        public string ItemShtName2 = "";
        public string ItemCode = "";
        public int value = 1;
        public string ProductionId = "";
        public string BatchNo = "";

        public Package()
        {
            try
            {
                InitializeComponent();
                serial = objCom.COM();
                serial.DataReceived += new SerialDataReceivedEventHandler(ReadIn);
                this.dfnProc1Update = new dlProc1DataRecv(Proc1DataProcess);
                this.dfnProcComClose = new dlProcComClose(fnComClose);
                btnStop.IsEnabled = btnEnter.IsEnabled = false;

                if (serial.IsOpen == true)
                    serial.Close();
                
                CloseAll();
                LoadShift();
                LoadItems();
                LoadGrid();
            }
            catch (Exception Ex)
            {
                lblMsg.Content = Ex.Message.ToString();
            }
        }

        private void Proc1DataProcess(string com)
        {
            try
            {
                lblMsg.Content = "";
                txtWeight.Text = com;
                txtWeight.Text = com.Replace("?", "");
                if (Convert.ToDecimal(txtWeight.Text) == 0)
                    Globals.WeightFlag = 0;
            }
            catch (Exception Ex)
            {
                lblMsg.Content = Ex.Message.ToString() + com;
            }
        }

        private void fnComClose()
        {
            if (serial.IsOpen == true)
                serial.Close();
        }

        private void LoadItems()
        {
            try
            {
                string buf = "";
                lblMsg.Content = "";
                gdImage.Children.Clear();
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable(" Select Distinct(a.MaterialCode) AS ItemCode,b.MaterialDesc as ItemName from Production_header a,material_master b Where a.Status in (1,2) " +
                    " AND a.materialCode=b.materialCode AND Date(a.PdnDate)=CurDate() AND b.Status in (1,2) AND a.Status in (1,2) AND a.DocFlag=0 AND a.System='" +
                    Globals.System + "' AND a.UnitId='" + Globals.Unit + "' AND a.PlantCode='" + Globals.Plant + "' AND a.JobFlag=0 Order By b.materialdesc");
                if (dt == null)
                {
                    MessageBox.Show("Error - A[3,0] " + objMas.DBErrBuf);
                    return;
                }

                   Color[] colur = new Color[] { Colors.AliceBlue, Colors.AntiqueWhite,Colors.Aqua,Colors.Aquamarine,Colors.Azure,Colors.Beige,Colors.Bisque,
                   Colors.BlanchedAlmond,Colors.Blue,Colors.BlueViolet,Colors.Brown,Colors.BurlyWood,Colors.CadetBlue,Colors.Chocolate,Colors.Coral,
                   Colors.CornflowerBlue,Colors.Cornsilk,Colors.Crimson,Colors.Cyan,Colors.DarkBlue,Colors.DarkCyan,Colors.DarkGoldenrod,Colors.DarkGray,
                   Colors.DarkGreen,Colors.DarkKhaki,Colors.DarkMagenta,Colors.DarkOliveGreen,Colors.DarkOrange,Colors.DarkOrchid,Colors.DarkRed,Colors.DarkSalmon,
                   Colors.DarkSeaGreen,Colors.DarkSlateBlue,Colors.DarkSlateGray,Colors.DarkTurquoise,Colors.DarkViolet,Colors.DeepPink,Colors.DeepSkyBlue,
                   Colors.DimGray,Colors.DodgerBlue,Colors.Firebrick,Colors.FloralWhite,Colors.ForestGreen,Colors.Fuchsia,Colors.Gainsboro,Colors.GhostWhite,Colors.Gold};


                int j = 0, k = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    buf = dt.Rows[i]["ItemName"].ToString();
                    if (dt.Rows[i]["ItemName"].ToString().Length >= 20)
                    {
                        buf = dt.Rows[i]["ItemName"].ToString().Substring(0, 20);
                        buf += "\n" + dt.Rows[i]["ItemName"].ToString().Substring(20);
                    }
                    RowDefinition gridRow1 = new RowDefinition();
                    gridRow1.Height = new GridLength(90);
                    gdImage.RowDefinitions.Add(gridRow1);
                    Button btnImage = new Button();
                    btnImage.Content = buf;
                    btnImage.Tag = dt.Rows[i]["ItemCode"].ToString();
                    btnImage.Click += gdvItembtn_Click;
                    btnImage.FontSize = 14;
                    btnImage.FontWeight = FontWeights.Bold;
                    btnImage.Background = new SolidColorBrush(colur[i]);
                    btnImage.VerticalAlignment = VerticalAlignment.Top;
                    btnImage.Height = 70;
                    btnImage.Width = 180;
                    Grid.SetRow(btnImage, k);
                    Grid.SetColumn(btnImage, j);
                    gdImage.Children.Add(btnImage);
                    j++;
                    if (j == 3)
                    {
                        k++;
                        j = 0;
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[3,0]" + Ex.Message.ToString());
            }
        }

        private void LoadShift()
        {
            try
            {
                string buf = "";
                Int64 Ret = objMas.GetTableCount("shift_master WHERE ShiftStatus=1 AND SystemId = '" + Globals.System + "' AND UnitId='" +
                    Globals.Unit + "' and PlantCode='" + Globals.Plant + "'");
                if (Ret >= 1)
                {
                    Ret = objMas.ExecScalar("Select ShiftName FROM shift_master WHERE ShiftStatus = 1 AND SystemId ='" + Globals.System + "' AND UnitId='" +
                                            Globals.Unit + "' AND PlantCode='" + Globals.Plant + "'", ref buf);
                    lblShiftDetails.Content = buf;
                    lblShiftDetails.Visibility = lblShift.Visibility = Visibility.Visible;
                }
                else
                    lblShiftDetails.Visibility = lblShift.Visibility = Visibility.Hidden;
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[] " + Ex.Message.ToString());
            }
        }

        private void gdvItembtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lblMsg.Content = "";
                if (btnStart.IsEnabled == false)
                    return;

                Int64 Ret = objMas.CheckShiftOpen(Globals.System);
                if (Ret <= 0)
                {
                    MessageBox.Show("Open Shift First");
                    return;
                }
                foreach (Button btnold in FindVisualChildren<Button>(gdImage))
                {
                    ItemCode = "";
                    btnold.IsEnabled = true;
                }
                DataTable dt = new DataTable();
                Button btn = (Button)sender;
                btn.IsEnabled = false;
                ItemCode = btn.Tag.ToString();
                ItemShtName1 = ItemShtName2 = "";

                dt = objMas.GetDataTable("Select ItemShortName1,ItemShortName2 from material_master Where materialCode='" + ItemCode + "'");
                if (dt == null)
                {
                    MessageBox.Show("Error - A[3,0] Failed To Fetch Short Name " + objMas.DBErrBuf);
                    return;
                }
                else if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Add Short Name For Items in Item Master");
                    return;
                }
                else if (dt.Rows.Count != 0)
                {
                    if (dt.Rows[0]["ItemShortName1"].ToString() != "")
                        ItemShtName1 = dt.Rows[0]["ItemShortName1"].ToString();
                    if (dt.Rows[0]["ItemShortName2"].ToString() != "")
                        ItemShtName2 = dt.Rows[0]["ItemShortName2"].ToString();
                }
                if (ItemShtName1 == "")
                {
                    MessageBox.Show("Add Short Name For Items in Item Master");
                    return;
                }
               
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[3,1] " + Ex.Message.ToString());
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int Ret = 0;
                string buf = "";
                txtWeight.Text = txtQuantity.Text = "";
                DataTable dt = new DataTable();
                if (ItemCode == "")
                {
                    MessageBox.Show("Select Items");
                    return;
                }
                dt = objMas.GetDataTable("Select PDNId,BatchNo,TargetQty from Production_header Where materialCode='" + ItemCode + "' AND Date(PDNDate)=CurDate() AND JobFlag=0 " +
                    " AND System='" + Globals.System + "' AND UnitId='" + Globals.Unit + "' AND PlantCode='" + Globals.Plant + "'");
                if (dt == null)
                {
                    MessageBox.Show("Error -A[3,2] " + objMas.DBErrBuf);
                    return;
                }
                else if (dt.Rows.Count != 0)
                {
                    ProductionId = dt.Rows[0]["PDNId"].ToString();
                    BatchNo = dt.Rows[0]["BatchNo"].ToString();

                    Ret = objMas.ExecScalar("Select ifnull(Sum(PDNCnt),0) from Production_header Where materialCode='" + ItemCode
                        + "' AND Date(PDNDate)=CurDate() AND JobFlag=0 and PdnId='" + ProductionId + "'", ref buf);

                    if (Convert.ToDecimal(buf) >= Convert.ToDecimal(dt.Rows[0]["TargetQty"].ToString()))
                    {
                        Ret = objMas.ExecQry("Update Production_header set JobFlag=1 Where ItemCode='" + ItemCode
                        + "' AND Date(PDNDate)=CurDate() AND JobFlag=0 and PdnId='" + ProductionId + "'");
                        lblMsg.Content = "Target Quantity Reached For this Item.";
                        return;
                    }
                }
                dt.Dispose();

                Ret = objMas.ExecQry(" Update Production_header Set StartEndFlag = 1 Where MaterialCode='" + ItemCode + "' AND Date(PDNDate)=CurDate() AND DocFlag = 0 AND System='" +
                                        Globals.System + "' AND UnitId='" + Globals.Unit + "' AND PlantCode='" + Globals.Plant + "'");
                if (Ret >= 1)
                {
                    btnStart.IsEnabled = false;
                    btnStop.IsEnabled = btnEnter.IsEnabled = true;
                    lblMsg.Content = "Machines Started.... ";
                    Globals.StartEndFlag = 1;
                    Globals.WeightFlag = 0;
                    btnEnter.Focus();
                    if (Globals.OTA_TESTING == false)
                    {
                        if (serial.IsOpen == false)
                            serial.Open();
                    }
                    else
                        txtWeight.IsEnabled = true;

                    LoadGrid();
                    LoadNetWeight();
                }
                else
                {
                    MessageBox.Show("Error -A[3,2] Failed To Start Machines");
                    return;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[3,3] " + Ex.Message.ToString());
                return;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int Ret = 0;

                btnStop.IsEnabled = btnEnter.IsEnabled = false;
                btnStart.IsEnabled = true;
                value = 1;
                txtQuantity.Text = txtWeight.Text = "";

                lblMsg.Content = "Serial stopped";
                Ret = objMas.ExecQry(" Update Production_header Set StartEndFlag = 0 Where MaterialCode = '" + ItemCode + "' AND Date(PDNDate)=CurDate() AND DocFlag=0 AND System='" +
                                      Globals.System + "' AND UnitId='" + Globals.Unit + "' AND PlantCode='" + Globals.Plant + "'");
                if (Ret >= 1)
                {
                    foreach (Button btnItem in FindVisualChildren<Button>(gdImage))
                        btnItem.IsEnabled = true;

                    Globals.StartEndFlag = 0;
                    ItemCode =  "";
                    LoadGrid();
                    LoadNetWeight();
                    return;
                }
                else
                {
                    MessageBox.Show("Error - A[3,3] Failed To Stop Machines");
                    return;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[3,4] " + Ex.Message.ToString());
                return;
            }
        }

        private void ShowData(string Data)
        {
            try
            {
                if (serial.IsOpen == false) return;

                Dispatcher.Invoke(this.dfnProc1Update, new object[] { Data });
            }
            catch (Exception Ex)
            {
                lblMsg.Content = "E1 : " + Ex.Message.ToString();
            }
        }

        private void ReadIn(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serial.IsOpen == false) return;
                string indata = serial.ReadTo("\r");
                ShowData(indata);
            }
            catch (Exception ex)
            {
                lblMsg.Content = "Invk : " + ex.Message.ToString();
            }
        }

        public void CloseAll()
        {
            Dispatcher.Invoke(this.dfnProcComClose);
            Window objWin = Window.GetWindow(this);
            foreach (Window openWin in System.Windows.Application.Current.Windows)
            {
                if (openWin != objWin)
                    openWin.Close();
            }
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Globals.WeightFlag == 1)
                {
                    MsgErr frmErr = new MsgErr();
                    frmErr.ShowDialog();
                    return;
                }
                lblMsg.Content = "";
                string BgWt, MinTolerance, MaxTolerance, ItemName, RunningNo, Month, MRP, ExpDate, PackDate, Unit, NetWgt, mfgMonth, buf, PdnCnt, JobClose;
                BgWt = MinTolerance = MaxTolerance = ItemName = RunningNo = Month = MRP = ExpDate = PackDate = Unit = NetWgt = mfgMonth = buf = PdnCnt = JobClose = "";
                string[] MonthYear = new string[2];

                Int64 Ret = 0, ItemCount = 0;
                DataTable dt = new DataTable();
                buf = txtWeight.Text.Replace("\r", "");
                buf = buf.Replace("?", "");
                buf = Convert.ToDecimal(buf).ToString("#.##");

                if (txtWeight.Text == "")
                {
                    MessageBox.Show("Weight Can't be Empty");
                    return;
                }
                if (buf == "0.00" || buf == "0" || buf == "000.000" || buf == "00.00" || buf == "000.00" || buf == "")
                {
                    lblMsg.Content = "Input Valid Weight";
                    return;
                }
                dt = objMas.GetDataTable("Select GrossWgt,MinTolerance,MaxTolerance,NetWgt from material_master Where materialCode='" + ItemCode + "'");
                if (dt == null)
                {
                    lblMsg.Content = "Error " + objMas.DBErrBuf;
                    return;
                }
                BgWt = dt.Rows[0]["GrossWgt"].ToString();
                NetWgt = dt.Rows[0]["NetWgt"].ToString();
                MinTolerance = dt.Rows[0]["MinTolerance"].ToString();
                MaxTolerance = dt.Rows[0]["MaxTolerance"].ToString();
                
                if (BgWt == "" || Convert.ToDecimal(BgWt) == 0)
                {
                    lblMsg.Content = "Input Valid Weight in Material Master";
                    return;
                }
                if (Convert.ToDecimal(buf) < (Convert.ToDecimal(BgWt) - Convert.ToDecimal(MinTolerance)))
                {
                    ErrorBox frmErr = new ErrorBox();
                    frmErr.ShowDialog();
                    return;
                }
                if (Convert.ToDecimal(buf) > (Convert.ToDecimal(BgWt) + Convert.ToDecimal(MaxTolerance)))
                {
                    ErrorBox frmErr = new ErrorBox();
                    frmErr.ShowDialog();
                    return;
                }
                txtQuantity.Text = value.ToString();
                dt.Dispose();

                dt = objMas.GetDataTable(" Select Distinct(b.MonthYear) AS MonthYear,DateFMT,IFNull(c.RunningNo + 1,0) AS RunningNo,Date_Format(ExpDate,'%d-%b-%Y') AS " +
                    " ExpDate,Date_Format(PackDate,'%d-%b-%Y') AS PackDate,UnitId,c.MaterialDesc as ItemName,Concat(c.ProdGroupCode,c.SizeInInchCode) as ItemAlias," +
                    " b.BatchNo,PlantFmt,b.TargetQty FROM admin_settings a,production_header b,material_master c Where Date(b.PDnDate)=CurDate() AND b.MaterialCode='" +
                    ItemCode + "' AND b.PdnId='" + ProductionId + "' AND b.materialCode=c.materialCode ");

                if (dt == null)
                {
                    lblMsg.Content = "Error - A[] " + objMas.DBErrBuf;
                    return;
                }
                Ret = objMas.ExecScalar("Select PDNCnt from Production_header Where MaterialCode='" + ItemCode
                        + "' AND Date(PDNDate)=CurDate() AND PdnId='" + ProductionId + "' ", ref PdnCnt);

                if (Convert.ToDecimal(PdnCnt) >= Convert.ToDecimal(dt.Rows[0]["TargetQty"].ToString()))
                {
                    lblMsg.Content = "Target Quantity Reached For this Item.";
                    return;
                }

                Month = dt.Rows[0]["MonthYear"].ToString();
                RunningNo = dt.Rows[0]["RunningNo"].ToString();
                RunningNo = BatchNo + RunningNo.PadLeft(4, '0');
                ExpDate = dt.Rows[0]["ExpDate"].ToString(); 
                PackDate = dt.Rows[0]["PackDate"].ToString(); 
                Unit = dt.Rows[0]["UnitId"].ToString();

                string[] sQry = new string[4];
                //Ret = objMas.GetTableCount(" stock_master Where Date_format(Month,'%b,%Y') = '" + dt.Rows[0]["MonthYear"].ToString()
                //                            + "' And ItemCode = '" + ItemCode + "'");
                //if (Ret == 0)
                //{
                //    MonthYear = dt.Rows[0]["MonthYear"].ToString().Split(',');
                //    mfgMonth = MonthYear[1] + "-" + DateTime.ParseExact(MonthYear[0], "MMM", CultureInfo.CurrentCulture).Month.ToString("00") + "-01";

                //    sQry[0] = " Insert into stock_master(ItemCode,Month,Qty,Status)Values('" + ItemCode + "','" + mfgMonth + "',1,1)";
                //}
                //else
                //    sQry[0] = " Update stock_master set Qty=Qty + 1 Where ItemCode='" + ItemCode + "' and Date_format(Month,'%b,%Y') = '" +
                //                dt.Rows[0]["MonthYear"].ToString() + "'";

                //Ret = objMas.GetId(" Select Count(1) + 1 From production_lines Where Date(TranDate)=CurDate() AND ItemCode='" + ItemCode
                //                    + "' AND System='" + Globals.System + "'", ref ItemCount);


                sQry[0] = " Insert into Production_lines(PDNId,System,TranDate,Barcode,Weight,MaterialCode,MonthYear,BatchNo,Status,BoxNo)Values('" + ProductionId
                          + "','" + Globals.System + "',NOW(),'" + RunningNo + "','" + buf + "','" + ItemCode + "','" +
                          dt.Rows[0]["MonthYear"].ToString() + "','" + dt.Rows[0]["BatchNo"].ToString() + "',1,'" + ItemCount + "')";

                if (Convert.ToDecimal(PdnCnt) + 1 == Convert.ToDecimal(dt.Rows[0]["TargetQty"].ToString()))
                    JobClose = ",JobFlag = 1 ";
                else
                    JobClose = "";

                sQry[1] = " Update Production_header Set PdnCnt = PDNCnt + 1 " + JobClose + " Where PDNId='" + ProductionId + "' AND MaterialCode='" + ItemCode
                          + "' AND UnitId='" + Globals.Unit + "' AND System='" + Globals.System + "' AND PlantCode='" + Globals.Plant + "'";

                sQry[2] = " Update material_master Set RunningNo = RunningNo + 1 where MaterialCode='" + ItemCode + "'";

                Ret = objMas.GetTableCount("material_master where MaterialCode='" + ItemCode + "' and RunningNo='9999'");
                if (Ret >= 1)
                    sQry[3] = "Update material_master Set RunningNo = 0 where MaterialCode='" + ItemCode + "'";

                Ret = objMas.ExecMultipleQry(Ret == 1 ? 4 : 3, sQry);
                if (Ret <= 0)
                {
                    MessageBox.Show("Failed To Print Barcode Label" + objMas.DBErrBuf);
                    return;
                }
                else
                {
                    
                    lblMsg.Content = "Printing Label....";
                }

                PrintLabel(ItemCode, buf, BatchNo, dt.Rows[0]["ItemName"].ToString(), RunningNo, Month, MRP, ExpDate, PackDate, ItemCount, Unit, NetWgt);
                LoadGrid();
                LoadNetWeight();
                dt.Dispose();
                Globals.WeightFlag = 1;
                PrintingBox MsgBox = new PrintingBox();
                MsgBox.ShowDialog();
                value++;
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[3,5] " + Ex.Message.ToString());
            }
        }

        private void LoadGrid()
        {
            try
            {
                gdvPackage.ItemsSource = null;
                DataTable dbDt = new DataTable();
                DataTable dt = new DataTable("GdvItems");
                string sQry = " Select Distinct a.materialCode as ItemCode ,b.materialDesc as ItemName,a.BatchNo,IfNull(a.PdnCnt,0) AS PdnCnt from " +
                              " production_header a,material_master b Where a.MaterialCode = b.MaterialCode AND Date(a.PDNDate)=CurDate() AND " +
                              " a.System='" + Globals.System + "' AND a.UnitId='" + Globals.Unit + "' AND a.JobFlag=0 AND PdnCnt <> 0 AND a.PlantCode='" +
                              Globals.Plant + "' and a.materialCode='" + ItemCode + "'  Group By a.MaterialCode";

                dt = objMas.GetDataTable(sQry);
                if (dt == null)
                {
                    lblMsg.Content = "Error - A[3,4] " + objMas.DBErrBuf;
                    return;
                }
                if (dt.Rows.Count != 0)
                    this.gdvPackage.ItemsSource = dt.DefaultView;
            }
            catch (Exception Ex)
            {
                lblMsg.Content = "" + Ex.Message.ToString();
            }
        }

        private void PrintLabel(string ItemCode, string Weight, string BatchNo, string ItemName, string RunningNo, string Month, string MRP,
                                string ExpDate, string PKDDate, Int64 ItemCount, string Unit, string NetWgt)
        {
            try
            {
                string buf = "";
                string Barcode = RunningNo;
                string[] Mnth = new string[2];
                Mnth = Month.Split(',');

                if (File.Exists(ConfigurationManager.AppSettings["PRNFILE1"]) == true)
                    buf = System.IO.File.ReadAllText(ConfigurationManager.AppSettings["PRNFILE1"]);
                else
                {
                    MessageBox.Show("PRN File 1 Not Found", buf);
                    return;
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

                objUsbpnt.SendStringToPrinter(Globals.PRINTER_NAME1, buf);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed To Print Barcode Label" + ex.Message.ToString());
            }
        }

        private void LoadNetWeight()
        {
            try
            {

                DataTable dt = objMas.GetDataTable(" Select a.PdnCnt AS PdnCnt,IFNULL(Sum(b.Weight),0) AS GrossWgt,a.TargetQty from Production_header a left join production_lines b on (" +
                               " a.PDNId=b.PDNId and a.JobFlag=0 and a.MaterialCode='" + ItemCode + "'  ANd Date(a.PDNDate)=CurDate() AND Date(b.TranDate)=CurDate() AND " +
                               "a.MaterialCode=b.MaterialCode AND a.System=b.System) where a.UnitId='" + Globals.Unit + "' AND a.PlantCode='" + Globals.Plant
                               + "' and a.MaterialCode='" + ItemCode + "' and a.JobFlag=0");

                if (dt == null)
                {
                    lblMsg.Content = "Error - A[]" + objMas.DBErrBuf;
                    return;
                }
                else
                {
                    txtGrossWeight.Text = dt.Rows[0]["GrossWgt"].ToString();
                    txtTotal.Text = dt.Rows[0]["PdnCnt"].ToString() + "/" + dt.Rows[0]["TargetQty"].ToString();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error - C[3,5] " + Ex.Message.ToString());
            }
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            Reprint MsgBox = new Reprint();
            MsgBox.ShowDialog();
        }

        private string ConvStrBytetoStr(string src)
        {
            Int64 i;
            string buf;

            i = 0;
            buf = "";

            i = Convert.ToInt64(src);
            buf = i.ToString("X");
            return buf;
        }

    }

}
