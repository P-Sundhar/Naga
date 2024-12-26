using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
using System.IO;
using log4net;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Globalization;
using iTextSharp.text.pdf;
using iTextSharp.text;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using NagaMaster.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Reflection;

public class MasterLogic
{
    MySqlConnection Con;

    ILog plogWriter = LogManager.GetLogger("FILE");
    public string DBErrBuf = "";
    const string PAGEDESC = "MasterLogic";


    // Pdf Font Size
    public static int PDF_FONT_SMALL = 1;
    public static int PDF_FONT_SMALL_BOLD = 1;
    public static int PDF_FONT_NORMAL = 3;
    public static int PDF_FONT_NORMAL_BOLD = 4;
    public static int PDF_FONT_HIGH = 5;
    public static int PDF_FONT_HIGH_BOLD = 6;

    // Report Data Styles [Pdf / Excel]
    public static int RPTSTYLE_STRING = 0;
    public static int RPTSTYLE_STRING_SHORT = 1;
    public static int RPTSTYLE_INT = 2;
    public static int RPTSTYLE_DECIMAL = 3;
    public static int RPTSTYLE_DECIMAL_ROUND = 4;
    public static int RPTSTYLE_DATE = 5;

    // Date Format
    public const int DTFMT_YYMD_TO_DMYY = 1;    // YYYYMMDD to DDMMYYYY
    public const int DTFMT_YYMD_TO_DMY = 2;     // YYYYMMDD to DDMMYY
    public const int DTFMT_DMYY_TO_YYMD = 3;    // DDMMYYYY to YYYYMMDD
    public const int DTFMT_DMYY_TO_YMD = 4;     // DDMMYYYY to YYMMDD
    public const int DTFMT_MMMYY_TO_YM = 5;   // MMMMYYYY to YYMM (January 2022 as 2022-01

    // ******************************* DB Related Functions *********************
    public MasterLogic()
    {
        string[] StrCon = ConfigurationManager.ConnectionStrings["MySqlConn"].ToString().Split(new char[] { ';', }, StringSplitOptions.RemoveEmptyEntries);

        string UserId = StrCon[2].Substring(StrCon[2].IndexOf("=") + 1).Trim();
        string Pwd = StrCon[3].Substring(StrCon[3].IndexOf("=") + 1).Trim();
        string pooling = StrCon[4].Substring(StrCon[4].IndexOf("=") + 1).Trim();
        string tmp = StrCon[0] + ";" + StrCon[1] + ";User=" + UserId + ";Password=" + Pwd + ";Pooling=" + pooling + ";";
        Con = new MySqlConnection(tmp);
    }

    public void PrintLog(string FnName, string Data)
    {
        try
        {
            plogWriter.Debug(FnName + " : " + Data);
        }
        catch { }
    }

    public int ChkDBConnection()
    {
        DBErrBuf = "";
        try
        {
            Con.Open();
            return 1;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[1] Database Connection Failed : " + Ex.Message.ToString();
            PrintLog("MasterLogic Error: ", DBErrBuf);
            return 0;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
            Con.Close();
        }
    }

    public int GetCount(string sQry, ref Int64 iDstVal)
    {
        try
        {
            iDstVal = 0;
            if (Con.State == ConnectionState.Closed)
                Con.Open();
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            iDstVal = Convert.ToInt32(cmd.ExecuteScalar());
            return 1;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[2] : " + Ex.Message.ToString();
            PrintLog("GetCount Error: ", DBErrBuf);
            return -1;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
            Con.Close();
        }
    }

    public DataSet GetDataSet(string sQry)
    {
        DataSet ds = new DataSet();

        DBErrBuf = "";
        try
        {
            Con.Open();
            plogWriter.Debug("MasterLogic Qry : " + sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(ds, "LoadDataBinding");
            return ds;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[7]: " + Ex.Message.ToString();
            plogWriter.Debug("MasterLogic Error: " + DBErrBuf);
            ds = null;
        }
        finally
        {
            Con.Close();
        }
        return ds;
    }


    public Int64 InsertMasterUpload(string sMaxIdQry, string[] sQryHead, string[] sQryTail, int sQryCount, ref string Id)
    {
        Int64 ret = 0;
        if (Con.State == ConnectionState.Closed)
        Con.Open();
        MySqlTransaction Tran = Con.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            string qry = "";
            MySqlCommand cmdQry;
            MySqlCommand cmdId = new MySqlCommand(sMaxIdQry, Con, Tran);
            Id = Convert.ToString(cmdId.ExecuteScalar());
            cmdId.Dispose();
            if (Id == "")
            {
                DBErrBuf = "DC[8,1]: Failed to Fetch Id";
                Tran.Rollback();
                return -1;
            }

            for (int i = 0; i < sQryCount; i++)
            {
                qry = "";
                if (sQryTail[i].ToString() != "-")
                    qry = sQryHead[i] + Id + sQryTail[i];
                else
                    qry = sQryHead[i].ToString();
                if (qry == "")
                    continue;
                PrintLog("MasterLogic Qry : ", qry);
                cmdQry = new MySqlCommand(qry, Con, Tran);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = 1;
            Tran.Commit();
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[3] : " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            Tran.Rollback();
            ret = -1;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
            Con.Close();
        }
        return ret;
    }

    public Int64 Uploadmaster(string sMaxIdQry, string[] sQryHead, string[] sQryTail, string[] sQryStrHead, string[] sQryStrTail, int sQryCount)
    {
        Int64 ret = 0;
        if (Con.State == ConnectionState.Closed)
        Con.Open();
        MySqlTransaction Tran = Con.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            string Id = "", qry = "";
            MySqlCommand cmdQry;
            MySqlCommand cmdId;

            for (int i = 0; i < sQryCount; i++)
            {
                PrintLog("MasterLogic Qry : ", sMaxIdQry);
                cmdId = new MySqlCommand(sMaxIdQry, Con);
                Id = Convert.ToString(cmdId.ExecuteScalar());
                if (Id == "")
                {
                    DBErrBuf = "DC[15,1]: Failed to Fetch Id";
                    PrintLog("MasterLogic Error : ", DBErrBuf);
                    Tran.Rollback();
                    return -1;
                }
                qry = "";
                qry = sQryHead[i] + Id + sQryTail[i];
                if (qry == "")
                    continue;
                PrintLog("MasterLogic Qry : ", qry);
                cmdQry = new MySqlCommand(qry, Con);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();

                qry = "";
                qry = sQryStrHead[i] + Id + sQryStrTail[i];
                if (qry == "")
                    continue;
                PrintLog("MasterLogic Qry : ", qry);

                cmdQry = new MySqlCommand(qry, Con);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = Convert.ToInt64(Id);
            Tran.Commit();
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[4]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            Tran.Rollback();
            ret = -1;

        }
        finally
        {
            if (Con.State == ConnectionState.Open)
            Con.Close();
        }
        return ret;
    }



    // Page Editing Options
    public int ChkPageEditOpt(int MenuId, string UserId, Label lblMsg)
    {
        try
        {
            int ret = 0;
            if (Con.State == ConnectionState.Closed)
            Con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT Edit FROM usermenu_mapping WHERE UserId='" + UserId + "' AND  MenuId=" + MenuId + "", Con);
            ret = Convert.ToInt16(cmd.ExecuteScalar());
            return ret;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[5]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            lblMsg.Text = DBErrBuf + Ex.Message.ToString();
            return -1;
        }
        finally
        {
            Con.Close();
        }
    }

    public string FDBS(string SrcStr)
    {
        string DstBuf = "";

        DstBuf = SrcStr.Replace("'", "''");
        return DstBuf;
    }

    public Int64 ExecMultipleQry(Int64 Cnt, string[] sQry)
    {
        Int64 AffRows = 0, i = 0;
        
        DBErrBuf = "";

        if (Con.State == ConnectionState.Closed)
            Con.Open();

        MySqlTransaction objMysT = Con.BeginTransaction();

        try
        {
            MySqlCommand cmd;
            for (i = 0; i < Cnt; i++)
            {
                if (sQry[i] == string.Empty || sQry[i] == "")
                    continue;
                PrintLog("MasterLogic Qry : ", sQry[i] + "    " + i);
                cmd = new MySqlCommand(sQry[i], Con,objMysT);
                AffRows += Convert.ToInt16(cmd.ExecuteNonQuery());
                cmd.Dispose();
            }
            objMysT.Commit();
        }

        catch(Exception Ex)
        {
            objMysT.Rollback();
            AffRows = -1;
            DBErrBuf = "DC[6]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
                Con.Close();
        }
        return AffRows;
    }

    public int FetchSingleColumn(string sQry, ref string buf)
    {
        DBErrBuf = "";
        int ret = -1;
        try
        {
           
            buf = "";
            if (Con.State == ConnectionState.Closed)
                Con.Open();
            PrintLog("MasterLogic Qry : ", sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            MySqlDataReader Reader = cmd.ExecuteReader();
            if (Reader.Read())
            {
                ret = 1;
                Reader.Close();
                buf = Convert.ToString(cmd.ExecuteScalar());
               
            }
            else
            {
                ret = 0;
                Reader.Close();
            }
            return ret;
        }
        catch(Exception Ex)
        {
            DBErrBuf = "DC[7]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            return -1;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
                Con.Close();
        }
    }

    public int GetId(string sQry, ref Int64 iDstVal)
    {
        DBErrBuf = "";
        try
        {
            iDstVal = 0;
            if (Con.State == ConnectionState.Closed)
                Con.Open();
            PrintLog("MasterLogic Qry : ", sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            iDstVal = Convert.ToInt32(cmd.ExecuteScalar());
            return 1;
        }
        catch(Exception Ex)
        {
            DBErrBuf = "DC[8]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            return -1;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
                Con.Close();
        }
    }
    public int ExecScalar(string sQry, ref string buf)
    {
        DBErrBuf = "";
        try
        {
            buf = "";
            Con.Open();
            plogWriter.Debug("MasterLogic Qry : " + sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            buf = cmd.ExecuteScalar().ToString();
            if (buf.Length == 0)
                return 0;

            return 1;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[3]: " + Ex.Message.ToString();
            plogWriter.Debug("MasterLogic Error: " + DBErrBuf);
            return -1;
        }
        finally
        {
            Con.Close();
        }
    }
    public Int64 GetTableCount(string sQry)
    {
        Int64 ret = 0;
        try
        {
            if (Con.State == ConnectionState.Closed)
            Con.Open();
            PrintLog("MasterLogic Qry : ", " SELECT COUNT(1) FROM " + sQry);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(1) FROM " + sQry, Con);
            ret = Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[9]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            ret = -1;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
                Con.Close();
        }
        return ret;
    }

    internal void ExecQry(object p)
    {
        throw new NotImplementedException();
    }

    public int ExecQry(string Qry)
    {
        int ret = 0;

        DBErrBuf = "";

        try
        {
            if (Con.State == ConnectionState.Closed)
                Con.Open();
            PrintLog("MasterLogic Qry : ",  Qry);
            MySqlCommand cmd = new MySqlCommand(Qry, Con);
            ret = cmd.ExecuteNonQuery();
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[10]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            ret = -1;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
                Con.Close();
        }
        return ret;
    }

    

    public DataTable GetDataTable(string sQry)
    {
        DataTable dt = new DataTable();

        DBErrBuf = "";
        try
        {
            if (Con.State == ConnectionState.Closed)
                Con.Open();

            PrintLog("MasterLogic Qry : ", sQry);

            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[11]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            dt = null;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
                Con.Close();
        }
        return dt;
    }

    public Int64 InsertMaster(string sMaxIdQry, string[] sQryHead, string[] sQryTail,int sQryCount)
    {
        Int64 ret = 0;
        if (Con.State == ConnectionState.Closed)
        Con.Open();
        //MySqlTransaction Tran = Con.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            string Id = "", qry = "";
            MySqlCommand cmdQry;
            MySqlCommand cmdId = new MySqlCommand(sMaxIdQry, Con);//, Tran
            PrintLog("MasterLogic Qry : ", sMaxIdQry);
            Id = Convert.ToString(cmdId.ExecuteScalar());
            cmdId.Dispose();
            if (Id == "")
            {
                DBErrBuf = "DC[8,1]: Failed to Fetch Id";
                PrintLog("MasterLogic Error : ", DBErrBuf);
                //Tran.Rollback();
                return -1;
            }

            for (int i = 0; i < sQryCount; i++)
            {
                qry = "";
                if (sQryTail[i].ToString() != "-")
                    qry = sQryHead[i] + Id + sQryTail[i];
                else
                    qry = sQryHead[i].ToString();
                if (qry == "")
                    continue;
                PrintLog("MasterLogic Qry : ", qry);
                cmdQry = new MySqlCommand(qry, Con);//, Tran
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = 1;
            //Tran.Commit();
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[12]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            //Tran.Rollback();
            ret = -1;

        }
        finally
        {
            if (Con.State == ConnectionState.Open)
                Con.Close();
        }

        return ret;
    }






    public string ModifyDateFmt(string SrcDate, int Fmt)
    {
        DBErrBuf = "";
        try
        {
            string buf = "";

            if (Fmt == DTFMT_DMYY_TO_YYMD)
                buf = SrcDate.Substring(6, 4) + SrcDate.Substring(2, 4) + SrcDate.Substring(0, 2);
            else if (Fmt == DTFMT_DMYY_TO_YMD)
                buf = SrcDate.Substring(8, 2) + SrcDate.Substring(2, 4) + SrcDate.Substring(0, 2);
            else if (Fmt == DTFMT_YYMD_TO_DMYY)
                buf = SrcDate.Substring(8, 2) + SrcDate.Substring(4, 4) + SrcDate.Substring(0, 4);
            else if (Fmt == DTFMT_YYMD_TO_DMY)
                buf = SrcDate.Substring(8, 2) + SrcDate.Substring(4, 4) + SrcDate.Substring(2, 2);
            else if (Fmt == DTFMT_MMMYY_TO_YM)
            {
                DateTime dt = DateTime.ParseExact(SrcDate, "MMMM-yyyy", CultureInfo.InvariantCulture);
                buf = dt.ToString("yyyy-MM");
            }

            return buf;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "MC[23]: " + Ex.Message;
            return "";
        }
    }



    //*********************************************************Session Login****************************************************

    public Int64 ChkSessionLogin(string UserId, string SessionId)
    {
        try
        {
            if (Con.State == ConnectionState.Closed)
                Con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(1) FROM Logins WHERE UserId='" + UserId + "' AND SessionId='" + SessionId + "' AND Status=1", Con);
            int ret = Convert.ToInt32(cmd.ExecuteScalar());
            Con.Close();
            return ret;
        }
        catch (Exception Ex)
        {
            Con.Close();
            DBErrBuf = "DC[13]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            return -1;
        }
        finally
        {
            if (Con.State == ConnectionState.Open)
                Con.Close();
        }
    }

    public Int64 UpdateSessionLogin(string UserId, string SessionId)
    {
        try
        {
            Int64 ret = 0;
            ret = GetTableCount("Logins WHERE UserId='" + UserId + "'");
            if (ret >= 1)
                ret = ExecQry("UPDATE Logins SET SessionId='" + SessionId + "',Status=1 WHERE UserId='" + UserId + "'");
            else
                ret = ExecQry("INSERT INTO Logins(SessionId,UserId,Status) VALUES ('" + SessionId + "','" + UserId + "',1);");

            if (ret >= 0)
                return 1;

            return 0;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[14]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            return -1;
        }
    }

    public int LogOutSession(string UserId, string SessionId)
    {
        try
        {
            int ret = 0;

            ret = ExecQry("UPDATE Logins SET Status=0 WHERE UserId='" + UserId + "' AND SessionId='" + SessionId + "' ");
            if (ret >= 0)
                return 1;

            return 0;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[15]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Error : ", DBErrBuf);
            return -1;
        }
    }


    public string FmtDBString(string Src)
    {
        try
        {
            string DstBuf = "";

            DstBuf = Src.Replace("'", "''");
            return DstBuf;
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[17]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Qry : ", DBErrBuf);
            return "";
        }
    }

    public string FmtDBDate(string Date)
    {
        try
        {
            string[] sSplit = Date.Split('-');
            if (sSplit.Length == 3)
                return sSplit[2] + "-" + sSplit[1] + "-" + sSplit[0];
            else
                return "";
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[18]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Qry : ", DBErrBuf);
            return "";
        }
    }

    public Int64 Uploadmaster(string sMaxIdQry, string[] sQryHead, string[] sQryTail, int sQryCount)
    {
        Int64 ret = 0;
        if (Con.State == ConnectionState.Closed)
        Con.Open();
        MySqlTransaction Tran = Con.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            string Id = "", qry = "";
            MySqlCommand cmdQry;
            MySqlCommand cmdId;


            for (int i = 0; i < sQryCount; i++)
            {
                PrintLog("MasterLogic Qry : ", sMaxIdQry);
                cmdId = new MySqlCommand(sMaxIdQry, Con);
                Id = Convert.ToString(cmdId.ExecuteScalar());
                if (Id == "")
                {
                    DBErrBuf = "DC[15,1]: Failed to Fetch Id";
                    PrintLog("MasterLogic Qry : ", DBErrBuf);
                    Tran.Rollback();
                    return -1;
                }
                qry = "";
                qry = sQryHead[i] + Id + sQryTail[i];
                if (qry == "")
                    continue;
                PrintLog("MasterLogic Qry : ", qry);
                cmdQry = new MySqlCommand(qry, Con);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = Convert.ToInt64(Id);
            Tran.Commit();
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[19]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Qry : ", DBErrBuf);
            Tran.Rollback();
            ret = -1;

        }
        finally
        {
            if (Con.State == ConnectionState.Open)
            Con.Close();
        }
        return ret;
    }

    //public List<Menus> LoadMenus()
    //{

    //    var mnulist = new List<Menus>();

    //    DataTable dt = GetDataTable("Select d.MenuType,c.MenuId,c.View as IsVisible from employee_master a, userroleheader b,userrolelines c,webuser_menu d,systemuser_master e" +
    //        " where c.MenuId=d.MenuId and b.Id = c.RoleId and a.EmpType=b.Id and a.EmpId=e.EmpId");
    //    if (dt == null)
    //        return null;
    //    else
    //    {
    //        foreach (DataRow dr in dt.Rows)
    //            mnulist.Add(new Menus
    //            {
    //                MenuType = Convert.ToInt16(dr["MenuType"]),
    //                MenuId = Convert.ToInt16(dr["MenuId"]),
    //                IsVisible = Convert.ToInt16(dr["IsVisible"])
    //            });
    //    }
    //    return mnulist;
    //}

    private DataTable GetDataTable(object p)
    {
        throw new NotImplementedException();
    }

    //public Int64 GetMenuEdit(int MenuId)
    //{
    //    Int64 ret = GetTableCount("employee_master a, userroleheader b,userrolelines c,webuser_menu d where c.MenuId=d.MenuId and b.Id = c.RoleId and " +
    //        "a.EmpType=b.Id and a.EmpId=1 and c.MenuId='"+ MenuId +"' and c.Edit= 1");

    //    return ret;
    //}
    public string DecodeData(string Data)
    {
        DBErrBuf = "";

        try
        {
            string buf = "";

            buf = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(Data));
            return buf;
        }

        catch (Exception Ex)
        {
            DBErrBuf = "UC[2]: " + Ex.Message.ToString();
            PrintLog("MoveBackupTable : ", DBErrBuf);
            return "";
        }
    }

    internal long InsertMaster(string v)
    {
        throw new NotImplementedException();
    }



    //*********************************************************Read Data from Excel File ***************************************

    public int GetDataTableExcel(string path, int ChkColHeader, string[] ColNames, ref DataTable Dt)
    {
        DBErrBuf = "";
        var rowNum = 0;
        try
        {
            int i = 0;
            string buf = "";
            bool hasHeader = false;


            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(path))
                {
                    pck.Load(stream);
                }
                var ws = pck.Workbook.Worksheets.First();

                // adjust it accordingly( i've mentioned that this is a simple approach)
                hasHeader = ChkColHeader == 1 ? true : false;

                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                    Dt.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));

                if (ChkColHeader == 1)
                {
                    i = 0;
                    foreach (DataColumn dc in Dt.Columns)
                    {
                        if (dc.ColumnName != ColNames[i])
                        {
                            DBErrBuf = "Column [" + i + "] Name Mismatch in Excel Sheet";
                            return 0;
                        }
                        i++;
                    }
                }

                var startRow = hasHeader ? 2 : 1;
                for (rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    var row = Dt.NewRow();
                    i = 0;
                    foreach (var cell in wsRow)
                    {
                        i++;
                        buf = cell.Text;
                        row[cell.Start.Column - 1] = cell.Text;
                        if (i >= Dt.Columns.Count)
                            break;
                    }
                    Dt.Rows.Add(row);
                }
                return 1;
            }
        }
        catch (Exception Ex)
        {
            DBErrBuf = "UC[10]: AT Row Number : " + rowNum + ", Error :" + Ex.Message;
            PrintLog(PAGEDESC, DBErrBuf);
            return 0;
        }
    }


    private void Excel_AddRow(ExcelWorksheet ws, int IncrementRow, ref int Row, int FCol, int TCol, bool FontBold, int FontSize,
                ExcelHorizontalAlignment EAlign, int IsBorder, Color BgClr, int DataType, string Data, string Formulaqry)
    {
        try
        {
            var cell = ws.Cells[Row, FCol];

            if (Formulaqry.Length != 0)
                cell.Formula = Formulaqry;
            else if (Data.Trim().Length != 0 && FCol == TCol)
            {
                if (DataType == RPTSTYLE_INT)
                {
                    cell.Style.Numberformat.Format = "##,##,##0";
                    cell.Value = Convert.ToInt32(Data);
                }
                else if (DataType == RPTSTYLE_DECIMAL)
                {
                    cell.Style.Numberformat.Format = "##,##,##0.00";
                    cell.Value = Convert.ToDecimal(Data);
                }
                else if (DataType == RPTSTYLE_DATE)
                {
                    cell.Style.Numberformat.Format = "DD-MM-YYYY";
                    cell.Value = Convert.ToDateTime(Data);
                }
                else
                    cell.Value = Convert.ToString(Data);
            }
            else
                cell.Value = Data;

            if (IsBorder == 1)
                if (FCol == TCol)
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                else
                    ws.Cells[Row, FCol, Row, TCol].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            if (FCol != TCol)
                ws.Cells[Row, FCol, Row, TCol].Merge = true;
            if (FontBold == true)
                ws.Cells[Row, FCol, Row, TCol].Style.Font.Bold = true;
            if (FCol != TCol)
                ws.Cells[Row, FCol, Row, TCol].Style.HorizontalAlignment = EAlign;
            if (FontSize != 0)
                ws.Cells[Row, FCol, Row, TCol].Style.Font.Size = FontSize;

            if (BgClr != Color.Empty)
            {
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(BgClr);
            }

            if (IncrementRow == 1)
                Row++;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "UC[8]: " + Ex.Message;
            PrintLog(PAGEDESC, DBErrBuf);
        }
    }


    private int GenerateDynamicExcel(string RptName, string Heading, int BorderReq, int[] ColDataType, int[] RptSubTotCols, int[] RptTotalCols,
                            string FromDate, string ToDate, string[] AddlnData, DataTable dtRpt)
    {
        /* Return --> 0 Error, 1 Success      [ DBErrBuf --> Error : Msg, Success : Report Full Path  ]
       * FromDate & ToDate --> Both Avail  : Report Period XX to XX
       *                       Only ToDate : Report Of : XX
       *                       Both Empty  : No Date Desc
       * BorderReq --> Border Required for Col Header, Data, Totals [ 0 Not Req,  1 Req ]
       * ColumnType --> Column Data Type [String, Decimal, Int, Date]
       * RptSubTotCols --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
       * RptTotalCols --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
       * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
       */

        DBErrBuf = "";
        try
        {
            int i, col, row, TotStCol, TotStRow, SubTotStRow, PrtSubTot;
            int[] LColTot = new int[ColDataType.Length];
            int[] LColSubTot = new int[ColDataType.Length];

            i = col = row = TotStCol = TotStRow = SubTotStRow = PrtSubTot = 0;
            Array.Clear(LColTot, 0, LColTot.Length);
            Array.Clear(LColSubTot, 0, LColSubTot.Length);

            if (RptName.Length == 0)
            {
                DBErrBuf = "CU[7]: Invalid File Name";
                return 0;
            }

            LColTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            LColSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            if (RptTotalCols != null)
                LColTot = RptTotalCols;
            if (RptSubTotCols != null)
                LColSubTot = RptSubTotCols;

            if (ColDataType.Length != dtRpt.Columns.Count || ColDataType.Length != LColTot.Length || ColDataType.Length != LColSubTot.Length)
            {
                DBErrBuf = "Invalid Report Columns";
                return 0;
            }

            row = col = 1;
            using (ExcelPackage ExlPack = new ExcelPackage())
            {
                //Here setting some document properties
                ExlPack.Workbook.Properties.Author = "Origin Technology Associates";
                ExlPack.Workbook.Properties.Title = Globals.gsAppTile;

                //Create a sheet
                ExlPack.Workbook.Worksheets.Add(Heading.Length == 0 ? "Sheet" : Heading);
                ExcelWorksheet ws = ExlPack.Workbook.Worksheets[0];
                ws.Name = "Sheet";     //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                // Aligning Header Details
                if (Globals.gsAppTile.Length != 0)
                    Excel_AddRow(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 18, ExcelHorizontalAlignment.Center, 0, Color.Empty, 0, Globals.gsAppTile, "");
                if (Heading.Length != 0)
                    Excel_AddRow(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 0, ExcelHorizontalAlignment.Center, 0, Color.Empty, 0, Heading, "");
                row++;
                Excel_AddRow(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 0, ExcelHorizontalAlignment.Right, 0, Color.Empty, 0,
                            "Generated On:" + System.DateTime.Now.ToString("dd-MM-yyyy HH:mm"), "");
                if (FromDate.Length != 0 || ToDate.Length != 0)
                    Excel_AddRow(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 0, ExcelHorizontalAlignment.Left, 0, Color.Empty,
                                0, "Report of : " + FromDate + (ToDate.Length != 0 && FromDate != ToDate ? "  To  " + ToDate : ""), "");
                if (AddlnData != null && AddlnData.Length != 0)
                {
                    row++;
                    for (i = 0; i < AddlnData.Length; i++)
                        if (AddlnData[i] != null && AddlnData[i].Length != 0)
                            Excel_AddRow(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 0, ExcelHorizontalAlignment.Left, 0, Color.Empty, 0, AddlnData[i], "");
                }

                row++;
                for (col = 0; col < dtRpt.Columns.Count; col++)
                    Excel_AddRow(ws, 0, ref row, col + 1, col + 1, false, 0, ExcelHorizontalAlignment.Left, BorderReq, Color.LightGray,
                                RPTSTYLE_STRING, dtRpt.Columns[col].ColumnName, "");
                row++;

                // Check Whether Total need to be shown
                TotStCol = TotStRow = SubTotStRow = 0;
                if (RptTotalCols != null)
                {
                    for (i = col = 0; i < LColTot.Length; i++)
                        col += (LColTot[i] == 1 ? 1 : 0);
                    if (col != 0)
                    {
                        TotStRow = row;
                        for (i = TotStCol = 0; i < LColTot.Length; i++)
                        {
                            if (LColTot[i] == 1)
                                break;
                            TotStCol++;
                        }

                        // Check Whether Sub Total need to be shown
                        if (RptSubTotCols != null)
                        {
                            for (i = col = 0; i < LColSubTot.Length; i++)
                                col += (LColSubTot[i] == 1 ? 1 : 0);
                            SubTotStRow = col == 0 ? 0 : row;
                        }
                    }
                }

                for (i = 0; i < dtRpt.Rows.Count; i++) // Adding Data into rows
                {
                    if (SubTotStRow != 0 && i != 0)
                    {
                        for (col = 0; col < TotStCol; col++)
                            if (LColSubTot[col] == 1 && dtRpt.Rows[i - 1][col].ToString() != dtRpt.Rows[i][col].ToString()) // cols not equal, print sub total
                            {
                                PrtSubTot = 1;
                                break;
                            }

                        if (PrtSubTot == 1)
                        {
                            Excel_AddRow(ws, 0, ref row, 1, TotStCol, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray,
                                        0, "Sub-Total :", "");
                            for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                                Excel_AddRow(ws, 0, ref row, col + 1, col + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq, Color.LightGray,
                                        ColDataType[col], "", LColSubTot[col] == 0 ? "" :
                                        "SUBTOTAL(9," + ws.Cells[SubTotStRow, col + 1].Address + ":" + ws.Cells[row - 1, col + 1].Address + ")");
                            row += 2;
                            SubTotStRow = row;
                        }
                    }

                    PrtSubTot = 0;
                    for (col = 0; col < dtRpt.Columns.Count; col++)
                        Excel_AddRow(ws, 0, ref row, col + 1, col + 1, false, 0, ExcelHorizontalAlignment.General, BorderReq, Color.Empty,
                                ColDataType[col], dtRpt.Rows[i][col].ToString(), "");
                    row++;
                }

                if (TotStRow != 0)
                {
                    if (SubTotStRow != 0)
                    {
                        Excel_AddRow(ws, 0, ref row, 1, TotStCol, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray,
                                              0, "Sub-Total :", "");
                        for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                            Excel_AddRow(ws, 0, ref row, col + 1, col + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq,
                                    Color.LightGray, ColDataType[col], "", LColSubTot[col] == 0 ? "" :
                                    "SUBTOTAL(9," + ws.Cells[SubTotStRow, col + 1].Address + ":" + ws.Cells[row - 1, col + 1].Address + ")");
                        row += 2;
                    }

                    Excel_AddRow(ws, 0, ref row, 1, TotStCol, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.FromArgb(173, 173, 173),
                                0, "Total :", "");
                    for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                        Excel_AddRow(ws, 0, ref row, col + 1, col + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq,
                                Color.FromArgb(173, 173, 173), ColDataType[col], "", LColTot[col] == 0 ? "" :
                                "SUBTOTAL(9," + ws.Cells[TotStRow, col + 1].Address + ":" + ws.Cells[row - 1, col + 1].Address + ")");
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                Byte[] bin = ExlPack.GetAsByteArray();
                File.WriteAllBytes(HttpContext.Current.Server.MapPath(RptName), bin);
                DBErrBuf = RptName;
                return 1;
            }
        }
        catch (Exception Ex)
        {
            DBErrBuf = "UC[8]: " + Ex.Message;
            return 0;
        }
    }


    public void Pdf_CellData(int IsBold, int ColSpan, int IsBorder, int CSpanHAlign, int DataType, float FnSize, float PadLeft, string Data,
                                    PdfPTable PdfTbls)
    {
        try
        {
            string buf = "";

            iTextSharp.text.Font fntSel;
            if (IsBold == 1)
                fntSel = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_BOLD, FnSize);
            else
                fntSel = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES, FnSize);

            if (ColSpan != 0)
                buf = Data;
            else if (Data.Trim().Length != 0)
            {
                if (DataType == RPTSTYLE_DECIMAL)
                    buf = String.Format("{0:0.00}", Convert.ToDecimal(Data));
                else if (DataType == RPTSTYLE_INT)
                    buf = String.Format("{0:0}", Convert.ToInt16(Data));
                else
                    buf = Data;
            }
            else
                buf = "  ";

            PdfPCell pclCol = new PdfPCell(new Phrase(buf, fntSel));
            if (ColSpan != 0)
                pclCol.Colspan = ColSpan;
            if (IsBorder == 0)
                pclCol.Border = 0;
            pclCol.PaddingLeft = PadLeft;
            pclCol.HorizontalAlignment = ColSpan != 0 ? CSpanHAlign : DataType == RPTSTYLE_INT || DataType == RPTSTYLE_DECIMAL ?
                            PdfPCell.ALIGN_RIGHT : PdfPCell.ALIGN_LEFT;
            PdfTbls.AddCell(pclCol);
        }
        catch { }
    }


    public void Pdd_NewLine(int ColSpan, int IsBorder, PdfPTable PdfTbls)
    {
        try
        {
            PdfPCell pclCol = new PdfPCell();
            if (ColSpan != 0)
                pclCol.Colspan = ColSpan;
            if (IsBorder == 0)
                pclCol.Border = 0;
            PdfTbls.AddCell(pclCol);
        }
        catch { }
    }
    public void AddLineInPdf(int Font, int ColSpan, int IsBorder, int HAlign, float PadLeft, string Data, PdfPTable PdfTbls)
    {
        try
        {
            iTextSharp.text.Font fntNormal = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_ROMAN, 9);
            //iTextSharp.text.Font fntBIG = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_ROMAN, 110);
            iTextSharp.text.Font fntBIG = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_ROMAN, 50);
            iTextSharp.text.Font fntBold = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_BOLD, 9);
            iTextSharp.text.Font fntBoldHigh = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_BOLD, 11);
            iTextSharp.text.Font fntSel = Font == 0 ? fntNormal : Font == 1 ? fntBold : Font == 8 ? fntBIG : fntBoldHigh;

            PdfPCell pclCol = new PdfPCell(new Phrase(Data, fntSel));
            if (ColSpan != 0)
                pclCol.Colspan = ColSpan;
            if (IsBorder == 0)
                pclCol.Border = 0;
            pclCol.PaddingLeft = PadLeft;
            pclCol.HorizontalAlignment = HAlign;
            PdfTbls.AddCell(pclCol);
        }
        catch { }
    }

    private int GenerateDynamicPdf(int IsLandscape, string RptName, string Heading, int[] ColDataType, int[] RptSubTotCols, int[] RptTotalCols,
                                string FromDate, string ToDate, string[] AddlnData, DataTable dtRpt)
    {
        /* Return --> 0 Error, 1 Success
             sUtilErrBuf --> Error - Msg, Success - Report Full Path;
         * ToDate & FromDate --> Both Avail  : Report Period XX to XX
         *                       Only ToDate : Report Of : XX
         *                       Both Empty  : No Date Desc
         * IsLandscape --> 1 Landscape mode, 0 Portrait Mode
         * PdfColWdt --> Pdf Column Widths
         * DataType --> String / Date [Left Align];   Int / Decimal [Right Align]
         * SubTotCol --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
         * TotalCol --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
         * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
         */

        DBErrBuf = "";
        try
        {
            int i, j, TotColSpan, TotReq, SubTotReq, PrtSubTot;
            float fnSize = 11;
            float[] PdfColWdt;
            int[] LColTot;
            int[] LColSubTot;
            double[] dTotal;
            double[] dSubTot;
            PdfPTable pdfTblHdl = new PdfPTable(dtRpt.Columns.Count);

            i = j = TotColSpan = TotReq = SubTotReq = PrtSubTot = 0;

            if (RptName.Length == 0)
            {
                DBErrBuf = "Invalid Report Path";
                return 0;
            }

            PdfColWdt = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 8f).ToArray();
            LColTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            LColSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            dTotal = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0.00).ToArray();
            dSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0.00).ToArray();

            if (RptTotalCols != null)
                LColTot = RptTotalCols;
            if (RptSubTotCols != null)
                LColSubTot = RptSubTotCols;

            if (ColDataType.Length != dtRpt.Columns.Count || LColTot.Length != dtRpt.Columns.Count || LColSubTot.Length != dtRpt.Columns.Count)
            {
                DBErrBuf = "Invalid Report Columns";
                return 0;
            }

            for (i = 0; i < dtRpt.Columns.Count; i++)
                PdfColWdt[i] = ColDataType[i] == RPTSTYLE_INT ? 3f : ColDataType[i] == RPTSTYLE_DECIMAL ? 4.5f : ColDataType[i] == RPTSTYLE_DATE ||
                                    ColDataType[i] == RPTSTYLE_STRING_SHORT ? 5.5f : 9f;

            pdfTblHdl.WidthPercentage = 100;
            pdfTblHdl.SetWidths(PdfColWdt);
            pdfTblHdl.HorizontalAlignment = 1;
            pdfTblHdl.SpacingBefore = 20f;
            pdfTblHdl.SpacingAfter = 30f;

            if (File.Exists(HttpContext.Current.Server.MapPath(RptName)) == true)
                File.Delete(HttpContext.Current.Server.MapPath(RptName));

            Document Dcmnt = new Document(PageSize.A4, 25, 20, 25, 20);
            if (IsLandscape == 1)
                Dcmnt.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

            PdfWriter pdfwr = PdfWriter.GetInstance(Dcmnt, new FileStream(HttpContext.Current.Server.MapPath(RptName), FileMode.Create));
            Dcmnt.Open();
            Pdf_CellData(1, PdfColWdt.Length, 0, PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, fnSize, 6f, Globals.gsAppTile, pdfTblHdl);

            if (Heading.Length != 0)
                Pdf_CellData(1, PdfColWdt.Length, 0, PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, fnSize, 6f, Heading, pdfTblHdl);
            Pdd_NewLine(PdfColWdt.Length, 0, pdfTblHdl);

            Pdf_CellData(0, PdfColWdt.Length, 0, PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, fnSize, 5f,
                            "Report Generated On : " + System.DateTime.Now.ToString("dd-MM-yyyy HH:mm"), pdfTblHdl);
            if (FromDate.Length != 0 || ToDate.Length != 0)
                Pdf_CellData(0, PdfColWdt.Length, 0, PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, fnSize, 5f, "Report Of : " + FromDate +
                                    (ToDate.Length != 0 && FromDate != ToDate ? "  To  " + ToDate : ""), pdfTblHdl);

            if (AddlnData != null && AddlnData.Length != 0)
            {
                for (i = 0; i < AddlnData.Length; i++)
                {
                    if (AddlnData[i] == null || AddlnData[i].Length == 0)
                        continue;
                    Pdf_CellData(0, PdfColWdt.Length, 0, PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, fnSize, 5f, AddlnData[i], pdfTblHdl);
                }
            }
            Pdd_NewLine(PdfColWdt.Length, 0, pdfTblHdl);

            // Check Whether Total need to be shown
            TotReq = SubTotReq = 0;
            if (RptTotalCols != null)
                for (i = TotReq = 0; i < LColTot.Length; i++)
                    TotReq += (LColTot[i] == 1 ? 1 : 0);
            if (TotReq != 0)
            {
                TotReq = 1;
                for (i = TotColSpan = 0; i < LColTot.Length; i++)
                {
                    if (LColTot[i] == 1)
                        break;
                    TotColSpan++;
                }

                // Check Whether Sub Total need to be shown
                if (RptSubTotCols != null)
                {
                    for (i = SubTotReq = 0; i < LColSubTot.Length; i++)
                        SubTotReq += (LColSubTot[i] == 1 ? 1 : 0);
                    SubTotReq = SubTotReq == 0 ? 0 : 1;
                }
            }

            for (i = 0; i < PdfColWdt.Length; i++) // Column Heading
                Pdf_CellData(1, 0, 1, PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, fnSize, 5f, dtRpt.Columns[i].ColumnName, pdfTblHdl);

            // Data
            for (i = PrtSubTot = 0; i < dtRpt.Rows.Count; i++)
            {
                if (SubTotReq == 1 && i != 0)
                {
                    for (j = 0; j < TotColSpan; j++)
                        if (LColSubTot[j] == 1 && dtRpt.Rows[i - 1][j].ToString() != dtRpt.Rows[i][j].ToString()) // cols not equal, print sub total
                        {
                            PrtSubTot = 1;
                            break;
                        }

                    if (PrtSubTot == 1)
                    {
                        Pdf_CellData(0, TotColSpan, 1, PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, fnSize, 5f, "Sub-Total", pdfTblHdl);
                        for (j = TotColSpan; j < PdfColWdt.Length; j++)
                            Pdf_CellData(0, 0, 1, PdfPCell.ALIGN_RIGHT, ColDataType[i], fnSize, 5f, LColSubTot[j] == 1 ? dSubTot[j].ToString() : "", pdfTblHdl);
                        Pdd_NewLine(PdfColWdt.Length, 1, pdfTblHdl);
                        Array.Clear(dSubTot, 0, dTotal.Length);
                    }
                }

                PrtSubTot = 0;
                for (j = 0; j < PdfColWdt.Length; j++)
                {
                    Pdf_CellData(0, 0, 1, PdfPCell.ALIGN_RIGHT, ColDataType[j], fnSize, 5f, dtRpt.Rows[i][j].ToString(), pdfTblHdl);
                    if (TotReq == 1 && LColTot[j] == 1)
                    {
                        dSubTot[j] += Convert.ToDouble(dtRpt.Rows[i][j]);
                        dTotal[j] += Convert.ToDouble(dtRpt.Rows[i][j]);
                    }
                }
            }   // end for

            if (TotReq == 1)
            {
                if (SubTotReq == 1)
                {
                    Pdf_CellData(0, TotColSpan, 1, PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, fnSize, 5f, "Sub-Total", pdfTblHdl);
                    for (j = TotColSpan; j < PdfColWdt.Length; j++)
                        Pdf_CellData(0, 0, 1, PdfPCell.ALIGN_RIGHT, ColDataType[j], fnSize, 5f, LColSubTot[j] == 1 ? dSubTot[j].ToString() : "", pdfTblHdl);
                    Pdd_NewLine(PdfColWdt.Length, 1, pdfTblHdl);
                }

                Pdf_CellData(1, TotColSpan, 1, PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, fnSize, 5f, "Total", pdfTblHdl);
                for (j = TotColSpan; j < PdfColWdt.Length; j++)
                    Pdf_CellData(1, 0, 1, PdfPCell.ALIGN_RIGHT, ColDataType[j], fnSize, 5f, LColTot[j] == 1 ? dTotal[j].ToString() : "", pdfTblHdl);
            }

            Dcmnt.Add(pdfTblHdl);
            Dcmnt.Close();
            DBErrBuf = RptName;
            return 1;
        }
        catch (Exception Ex)
        {
            DBErrBuf = Ex.Message;
            return 0;
        }
    }


    public int GenerateReport(int DwnldFmt, int PdfLandscape, int ExlIsBorder, string RptName, string Heading, int[] ColDataType, int[] RptSubTotCols,
                                int[] RptTotalCols, string FromDate, string ToDate, string[] AddlnData, DataTable dtRpt, float[] ColWidth)
    {
        /* Return --> 0 Error, 1 Success
             sUtilErrBuf --> Error - Msg, Success - Report Full Path;
         * ToDate & FromDate --> Both Avail  : Report Period XX to XX
         *                       Only ToDate : Report Of : XX
         *                       Both Empty  : No Date Desc
         * IsLandscape --> 1 Landscape mode, 0 Portrait Mode
         * PdfColWdt --> Pdf Column Widths
         * DataType --> String / Date [Left Align];   Int / Decimal [Right Align]
         * SubTotCol --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
         * TotalCol --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
         * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
         */

        DBErrBuf = "";
        try
        {
            int ret = 0;

            RptName = RptName.Trim();
            if (RptName.Length == 0)
            {
                DBErrBuf = "CU[7]: Invalid Report Name";
                return 0;
            }

            RptName = "../Reports/" + RptName + (RptName.EndsWith("_") == true ? "" : "_") + System.DateTime.Now.ToString("yyyyMMddHHmm") +
                                    (DwnldFmt == Globals.DWNLDFMT_EXCEL ? ".xlsx" : ".pdf");
            if (File.Exists(RptName))
                File.Delete(RptName);

            if (DwnldFmt == Globals.DWNLDFMT_EXCEL)
                ret = GenerateDynamicExcel(RptName, Heading, ExlIsBorder, ColDataType, RptSubTotCols, RptTotalCols, FromDate, ToDate, AddlnData, dtRpt);
            else
                ret = GenerateDynamicPdf(PdfLandscape, RptName, Heading, ColDataType, RptSubTotCols, RptTotalCols, FromDate, ToDate, AddlnData, dtRpt);

            return ret;
        }
        catch (Exception Ex)
        {
            DBErrBuf = Ex.Message;
            return 0;
        }
    }
    //********************************************************* Excel Report*******************************************************

    private void Excel_AddData(ExcelWorksheet ws, int IncrementRow, ref int Row, int FCol, int TCol, bool FontBold, int FontSize,
                    ExcelHorizontalAlignment EAlign, int IsBorder, Color BgClr, int DataType, string Data, string Formulaqry)
    {
        try
        {
            var cell = ws.Cells[Row, FCol];

            if (Formulaqry.Length != 0)
                cell.Formula = Formulaqry;
            else if (Data.Length != 0 && FCol == TCol)
            {
                if (DataType == RPTSTYLE_INT)
                {
                    cell.Style.Numberformat.Format = "##,##,##0";
                    cell.Value = Convert.ToInt32(Data);
                }
                else if (DataType == RPTSTYLE_DECIMAL)
                {
                    cell.Style.Numberformat.Format = "##,##,##0.00";
                    cell.Value = Convert.ToDecimal(Data);
                }
                else if (DataType == RPTSTYLE_DECIMAL_ROUND)
                {
                    cell.Style.Numberformat.Format = "##,##,##0";
                    cell.Value = Convert.ToDecimal(Data);
                }
                else if (DataType == RPTSTYLE_DATE)
                {
                    cell.Style.Numberformat.Format = "DD-MM-YYYY";
                    cell.Value = Convert.ToDateTime(Data);
                }
                else
                    cell.Value = Convert.ToString(Data);
            }
            else
                cell.Value = Data;

            if (IsBorder == 1)
                if (FCol == TCol)
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                else
                    ws.Cells[Row, FCol, Row, TCol].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            if (FCol != TCol)
                ws.Cells[Row, FCol, Row, TCol].Merge = true;
            if (FontBold == true)
                ws.Cells[Row, FCol, Row, TCol].Style.Font.Bold = true;
            if (FCol != TCol)
                ws.Cells[Row, FCol, Row, TCol].Style.HorizontalAlignment = EAlign;
            if (FontSize != 0)
                ws.Cells[Row, FCol, Row, TCol].Style.Font.Size = FontSize;

            if (BgClr != Color.Empty)
            {
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(BgClr);
            }

            if (IncrementRow == 1)
                Row++;
        }
        catch { }
    }


    private int GenDynamicExcel(string RptName, string Heading, int BorderReq, int[] ColDataType, int[] RptSubTotCols, int[] RptTotalCols,
                            string FromDate, string ToDate, string[] AddlnData, int[] Row1MergeCols, string[] Row1ColDesc, DataTable dtRpt)
    {
        /* Return --> 0 Error, 1 Success      [ DBErrBuf --> Error : Msg, Success : Report Full Path  ]
       * FromDate & ToDate --> Both Avail  : Report Period XX to XX
       *                       Only ToDate : Report Of : XX
       *                       Both Empty  : No Date Desc
       * BorderReq --> Border Required for Col Header, Data, Totals [ 0 Not Req,  1 Req ]
       * ColumnType --> Column Data Type [String, Decimal, Int, Date]
       * RptSubTotCols --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
       * RptTotalCols --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
       * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
       * Row1MergeCols --> Apart from header, if addtiional header (above dt header) need to shown with merging cols count  / null --> Dont Merge
       * Row1ColDesc --> Apart from header, if addtional header (above dt header) need to shown with merging cols Desc / null --> Dont Merge
       */

        DBErrBuf = "";
        try
        {
            int i, col, row, TotStCol, TotStRow, SubTotStRow, PrtSubTot;
            int[] LColTot = new int[ColDataType.Length];
            int[] LColSubTot = new int[ColDataType.Length];

            i = col = row = TotStCol = TotStRow = SubTotStRow = PrtSubTot = 0;
            Array.Clear(LColTot, 0, LColTot.Length);
            Array.Clear(LColSubTot, 0, LColSubTot.Length);

            if (RptName.Length == 0)
            {
                DBErrBuf = "MA[17]: Invalid File Name";
                return 0;
            }

            LColTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            LColSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            if (RptTotalCols != null)
                LColTot = RptTotalCols;
            if (RptSubTotCols != null)
                LColSubTot = RptSubTotCols;

            if (ColDataType.Length != dtRpt.Columns.Count || ColDataType.Length != LColTot.Length || ColDataType.Length != LColSubTot.Length)
            {
                DBErrBuf = "MA[18]: Invalid Report Columns";
                return 0;
            }

            if (Row1MergeCols != null)
            {
                if (Row1MergeCols.Length != Row1ColDesc.Length || !(Row1MergeCols.Length <= ColDataType.Length) ||
                            !(Row1ColDesc.Length <= ColDataType.Length))
                {
                    DBErrBuf = "MA[19]: Invalid Report Columns";
                    return 0;
                }

                for (i = row = 0; i < Row1MergeCols.Length; i++)
                    row += Row1MergeCols[i];

                if (row != ColDataType.Length)
                {
                    DBErrBuf = "MA[20]: Invalid Report Columns";
                    return 0;
                }
            }

            row = col = 1;

            using (ExcelPackage ExlPack = new ExcelPackage())
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                //Here setting some document properties
                ExlPack.Workbook.Properties.Author = "Origin Technology Associates";
                ExlPack.Workbook.Properties.Title = Globals.gsAppTile;

                //Create a sheet
                ExlPack.Workbook.Worksheets.Add(Heading);
                ExcelWorksheet ws = ExlPack.Workbook.Worksheets[0];
                ws.Name = "Sheet";     //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                // Aligning Header Details
                if (Globals.gsAppTile.Length != 0)
                    Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 18, ExcelHorizontalAlignment.Center, 0, Color.Empty, 0, Globals.gsAppTile, "");
                if (Heading.Length != 0)
                    Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 0, ExcelHorizontalAlignment.Center, 0, Color.Empty, 0, Heading, "");
                row++;
                Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 0, ExcelHorizontalAlignment.Right, 0, Color.Empty, 0,
                            "Generated On:" + System.DateTime.Now.ToString("dd-MM-yyyy HH:mm"), "");
                if (FromDate.Length != 0 || ToDate.Length != 0)
                    Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 0, ExcelHorizontalAlignment.Left, 0, Color.Empty,
                                0, "Report of : " + FromDate + (ToDate.Length != 0 && FromDate != ToDate ? "  To  " + ToDate : ""), "");
                if (AddlnData != null && AddlnData.Length != 0)
                {
                    row++;
                    for (i = 0; i < AddlnData.Length; i++)
                        if (AddlnData[i] != null && AddlnData[i].Length != 0)
                            Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count, true, 0, ExcelHorizontalAlignment.Left, 0, Color.Empty, 0, AddlnData[i], "");
                }

                if (Row1MergeCols != null)
                {
                    row++;
                    for (i = 0, col = 1; i < Row1MergeCols.Length; i++)
                    {
                        Excel_AddData(ws, 0, ref row, col, col + Row1MergeCols[i] - 1, true, 0, ExcelHorizontalAlignment.Center, BorderReq,
                                Color.LightGray, RPTSTYLE_STRING, Row1ColDesc[i], "");
                        col += Row1MergeCols[i];
                    }
                }

                row++;
                for (col = 0; col < dtRpt.Columns.Count; col++)
                    Excel_AddData(ws, 0, ref row, col + 1, col + 1, true, 0, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray,
                                RPTSTYLE_STRING, dtRpt.Columns[col].ColumnName, "");
                row++;

                // Check Whether Total need to be shown
                TotStCol = TotStRow = SubTotStRow = 0;
                if (RptTotalCols != null)
                {
                    for (i = col = 0; i < LColTot.Length; i++)
                        col += (LColTot[i] == 1 ? 1 : 0);
                    if (col != 0)
                    {
                        TotStRow = row;
                        for (i = TotStCol = 0; i < LColTot.Length; i++)
                        {
                            if (LColTot[i] == 1)
                                break;
                            TotStCol++;
                        }

                        // Check Whether Sub Total need to be shown
                        if (RptSubTotCols != null)
                        {
                            for (i = col = 0; i < LColSubTot.Length; i++)
                                col += (LColSubTot[i] == 1 ? 1 : 0);
                            SubTotStRow = col == 0 ? 0 : row;
                        }
                    }
                }

                for (i = 0; i < dtRpt.Rows.Count; i++) // Adding Data into rows
                {
                    if (SubTotStRow != 0 && i != 0)
                    {
                        for (col = 0; col < TotStCol; col++)
                            if (LColSubTot[col] == 1 && dtRpt.Rows[i - 1][col].ToString() != dtRpt.Rows[i][col].ToString()) // cols not equal, print sub total
                            {
                                PrtSubTot = 1;
                                break;
                            }

                        if (PrtSubTot == 1)
                        {
                            Excel_AddData(ws, 0, ref row, 1, TotStCol, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray,
                                        0, "Sub-Total :", "");
                            for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                                Excel_AddData(ws, 0, ref row, col + 1, col + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq, Color.LightGray,
                                        ColDataType[col], "", LColSubTot[col] == 0 ? "" :
                                        "SUBTOTAL(9," + ws.Cells[SubTotStRow, col + 1].Address + ":" + ws.Cells[row - 1, col + 1].Address + ")");
                            row += 2;
                            SubTotStRow = row;
                        }
                    }

                    PrtSubTot = 0;
                    for (col = 0; col < dtRpt.Columns.Count; col++)
                        Excel_AddData(ws, 0, ref row, col + 1, col + 1, false, 0, ExcelHorizontalAlignment.General, BorderReq, Color.Empty,
                                ColDataType[col], dtRpt.Rows[i][col].ToString(), "");
                    row++;
                }

                if (TotStRow != 0)
                {
                    if (SubTotStRow != 0)
                    {
                        Excel_AddData(ws, 0, ref row, 1, TotStCol, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray,
                                              0, "Sub-Total :", "");
                        for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                            Excel_AddData(ws, 0, ref row, col + 1, col + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq,
                                    Color.LightGray, ColDataType[col], "", LColSubTot[col] == 0 ? "" :
                                    "SUBTOTAL(9," + ws.Cells[SubTotStRow, col + 1].Address + ":" + ws.Cells[row - 1, col + 1].Address + ")");
                        row += 2;
                    }

                    Excel_AddData(ws, 0, ref row, 1, TotStCol, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.FromArgb(173, 173, 173),
                                0, "Total :", "");
                    for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                        Excel_AddData(ws, 0, ref row, col + 1, col + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq,
                                Color.FromArgb(173, 173, 173), ColDataType[col], "", LColTot[col] == 0 ? "" :
                                "SUBTOTAL(9," + ws.Cells[TotStRow, col + 1].Address + ":" + ws.Cells[row - 1, col + 1].Address + ")");
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                Byte[] bin = ExlPack.GetAsByteArray();
                File.WriteAllBytes(HttpContext.Current.Server.MapPath(RptName), bin);
                DBErrBuf = RptName;
                return 1;
            }
        }
        catch (Exception Ex)
        {
            DBErrBuf = "MC[42]: " + Ex.Message;
            return 0;
        }
    }


    //*********************************************************PDF Report*********************************************************\\


    private void Pdf_AddData(int ColSpan, int RowSpan, int FntSize, string BorderLRTB, int CSpanHAlign, int DataType, float PadLeft,
                    string Data, PdfPTable PdfTbls)
    {
        try
        {
            string buf = "";

            iTextSharp.text.Font fntSel;
            if (FntSize == PDF_FONT_NORMAL_BOLD)
                fntSel = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_BOLD, 11); // ,, iTextSharp.text.Font.UNDERLINE
            else if (FntSize == PDF_FONT_SMALL)
                fntSel = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES, 9);
            else if (FntSize == PDF_FONT_SMALL_BOLD)
                fntSel = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_BOLD, 9);
            else if (FntSize == PDF_FONT_HIGH)
                fntSel = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES, 13);
            else if (FntSize == PDF_FONT_HIGH_BOLD)
                fntSel = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES_BOLD, 13);
            else
                fntSel = iTextSharp.text.FontFactory.GetFont(FontFactory.TIMES, 11);

            if (ColSpan != 0)
                buf = Data;
            else
            {
                if (DataType == RPTSTYLE_DECIMAL)
                    buf = String.Format("{0:0.##}", Convert.ToDecimal(Data));
                else if (DataType == RPTSTYLE_DECIMAL_ROUND)
                    buf = String.Format("{0:0}", Convert.ToDecimal(Data));
                else if (DataType == RPTSTYLE_INT)
                    buf = String.Format("{0:0}", Convert.ToInt16(Data));
                else
                    buf = Data;
            }

            PdfPCell pclCol = new PdfPCell(new Phrase(buf, fntSel));
            if (ColSpan >= 2)
                pclCol.Colspan = ColSpan;
            if (RowSpan >= 2)
                pclCol.Rowspan = RowSpan;

            if (BorderLRTB.Length != 0)
            {
                pclCol.BorderWidthLeft = BorderLRTB[0] - '0' == 1 ? .5f : 0;
                pclCol.BorderWidthRight = BorderLRTB.Length >= 2 ? BorderLRTB[1] - '0' == 1 ? .5f : 0 : 0;
                pclCol.BorderWidthTop = BorderLRTB.Length >= 3 ? BorderLRTB[2] - '0' == 1 ? .5f : 0 : 0;
                pclCol.BorderWidthBottom = BorderLRTB.Length >= 4 ? BorderLRTB[3] - '0' == 1 ? .5f : 0 : 0;
            }
            else
                pclCol.Border = 0;

            pclCol.PaddingLeft = PadLeft;
            pclCol.HorizontalAlignment = ColSpan != 0 ? CSpanHAlign : DataType == RPTSTYLE_INT || DataType == RPTSTYLE_DECIMAL ||
                                            DataType == RPTSTYLE_DECIMAL_ROUND ? PdfPCell.ALIGN_LEFT : PdfPCell.ALIGN_LEFT;
            PdfTbls.AddCell(pclCol);
        }
        catch { }
    }


    private void Pdf_AddImage(int ColSpan, int RowSpan, string BorderLRTB, iTextSharp.text.Image Img, PdfPTable PdfTbls)
    {
        try
        {
            if (Img == null)
            {
                Pdf_AddData(ColSpan, RowSpan, PDF_FONT_NORMAL, BorderLRTB, PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, 5f, "", PdfTbls);
                return;
            }

            PdfPCell pclCol = new PdfPCell(Img);
            if (ColSpan >= 2)
                pclCol.Colspan = ColSpan;
            if (RowSpan >= 2)
                pclCol.Rowspan = RowSpan;
            if (BorderLRTB.Length != 0)
            {
                pclCol.BorderWidthLeft = BorderLRTB[0] - '0' == 1 ? .5f : 0;
                pclCol.BorderWidthRight = BorderLRTB.Length >= 2 ? BorderLRTB[1] - '0' == 1 ? .5f : 0 : 0;
                pclCol.BorderWidthTop = BorderLRTB.Length >= 3 ? BorderLRTB[2] - '0' == 1 ? .5f : 0 : 0;
                pclCol.BorderWidthBottom = BorderLRTB.Length >= 4 ? BorderLRTB[3] - '0' == 1 ? .5f : 0 : 0;
            }
            else
                pclCol.Border = 0;
            pclCol.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            PdfTbls.AddCell(pclCol);
        }
        catch { }
    }


    private void Pdf_AddNewLine(int ColSpan, int RowSpan, string BorderLRTB, PdfPTable PdfTbls)
    {
        try
        {
            PdfPCell pclCol = new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.TIMES, 9)));
            if (ColSpan >= 2)
                pclCol.Colspan = ColSpan;
            if (RowSpan >= 2)
                pclCol.Rowspan = RowSpan;
            if (BorderLRTB.Length != 0)
            {
                pclCol.BorderWidthLeft = BorderLRTB[0] - '0' == 1 ? .5f : 0;
                pclCol.BorderWidthRight = BorderLRTB.Length >= 2 ? BorderLRTB[1] - '0' == 1 ? .5f : 0 : 0;
                pclCol.BorderWidthTop = BorderLRTB.Length >= 3 ? BorderLRTB[2] - '0' == 1 ? .5f : 0 : 0;
                pclCol.BorderWidthBottom = BorderLRTB.Length >= 4 ? BorderLRTB[3] - '0' == 1 ? .5f : 0 : 0;
            }
            else
                pclCol.Border = 0;
            PdfTbls.AddCell(pclCol);
        }
        catch { }
    }


    private int GenDynamicPdf(string RptName, string Heading, int IsLandscape, int AutoSlNo, int[] ColDataType, int[] RptSubTotCols, int[] RptTotalCols,
                                string FromDate, string ToDate, string[] AddlnData,int[]AddlnDataColspan, int[] Row1MergeCols, string[] Row1ColDesc, DataTable dtRpt, float[] colwdth)
    {
        /* Return --> 0 Error, 1 Success   [ sUtilErrBuf --> Error - Msg, Success - Report Full Path  ] 
         * IsLandscape --> If Pdf ==> 1 Landscape mode, 0 Portrait Mode
         * AutoSlNo --> Show # in all Data Row Columns. [1 - Yes, 0 - NA]
         * RptName --> Name of the report. YYYYMMDDhhmmss + .xls/.pdf added with it
         * Heading --> Text to be printed as First Row
         * ColDataType --> Column's Data Type ==> Short String / String / Date [Left Align];   Int / Decimal [Right Align]
         * RptSubTotCols --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
         * RptTotalCols --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
         * From/To Date --> Both Avail  : Report Period XX to XX
         *                  Only ToDate : Report Of : XX
         *                  Both Empty  : No Date Desc
         * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
         * Row1MergeCols --> Apart from header, if addtiional header (above dt header) need to shown with merging cols count  / null --> Dont Merge
         * Row1ColDesc --> Apart from header, if addtional header (above dt header) need to shown with merging cols Desc / null --> Dont Merge
         */

        DBErrBuf = "";
        try
        {
            int i, j, k, Cols, TotColSpan, TotReq, SubTotReq, PrtSubTot;
            float[] PdfColWdt;
            int[] LColTot;
            int[] LColSubTot;
            double[] dTotal;
            double[] dSubTot;
            PdfPTable pdfTblHdl = new PdfPTable(dtRpt.Columns.Count + AutoSlNo);

            i = j = k = Cols = TotColSpan = TotReq = SubTotReq = PrtSubTot = 0;

            if (RptName.Length == 0)
            {
                DBErrBuf = "MA[21]: Invalid Report Path";
                return 0;
            }

            PdfColWdt = colwdth;
            LColTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            LColSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            dTotal = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0.00).ToArray();
            dSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0.00).ToArray();

            if (RptTotalCols != null)
                LColTot = RptTotalCols;
            if (RptSubTotCols != null)
                LColSubTot = RptSubTotCols;

            if (ColDataType.Length != dtRpt.Columns.Count || LColTot.Length != dtRpt.Columns.Count || LColSubTot.Length != dtRpt.Columns.Count)
            {
                DBErrBuf = "MA[22]: Invalid Report Columns";
                return 0;
            }

            if (Row1MergeCols != null)
            {
                if (Row1MergeCols.Length != Row1ColDesc.Length || !(Row1MergeCols.Length <= ColDataType.Length) ||
                            !(Row1ColDesc.Length <= ColDataType.Length))
                {
                    DBErrBuf = "MA[23]: Invalid Report Columns";
                    return 0;
                }

                for (i = j = 0; i < Row1MergeCols.Length; i++)
                    j += Row1MergeCols[i];

                if (j != ColDataType.Length)
                {
                    DBErrBuf = "MA[24]: Invalid Report Columns";
                    return 0;
                }
            }

            k = 0;
            if (AutoSlNo == 1)
                PdfColWdt[k++] = 3f;
            //for (i = 0; i < dtRpt.Columns.Count; i++, k++)
            //    PdfColWdt[k] = ColDataType[i] == RPTSTYLE_INT ? 3f : ColDataType[i] == RPTSTYLE_DECIMAL || ColDataType[i] == RPTSTYLE_DECIMAL_ROUND ? 4.5f :
            //                        ColDataType[i] == RPTSTYLE_DATE || ColDataType[i] == RPTSTYLE_STRING_SHORT ? 5.5f : 9f;

            Cols = PdfColWdt.Length;
            pdfTblHdl.WidthPercentage = 100;
            pdfTblHdl.SetWidths(PdfColWdt);
            pdfTblHdl.HorizontalAlignment = 1;
            pdfTblHdl.SpacingBefore = 20f;
            pdfTblHdl.SpacingAfter = 30f;

            if (File.Exists(HttpContext.Current.Server.MapPath(RptName)) == true)
                File.Delete(HttpContext.Current.Server.MapPath(RptName));

            Document Dcmnt = new Document(PageSize.A4, 25, 20, 25, 20);
            if (IsLandscape == 1)
                Dcmnt.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

            PdfWriter pdfwr = PdfWriter.GetInstance(Dcmnt, new FileStream(HttpContext.Current.Server.MapPath(RptName), FileMode.Create));
            Dcmnt.Open();

            if (Heading.Length != 0)
                Pdf_AddData(Cols, 1, PDF_FONT_HIGH, "", PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, 6f, Heading, pdfTblHdl);
            Pdf_AddNewLine(Cols, 1, "", pdfTblHdl);

            //Pdf_AddData(Cols, 1, PDF_FONT_NORMAL, "", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f, "Report Generated On : " +
            //            System.DateTime.Now.ToString("dd-MM-yyyy HH:mm"), pdfTblHdl);
            //Pdf_AddData(Cols, 1, PDF_FONT_NORMAL, "", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f,DocNo, pdfTblHdl);
            //Pdf_AddData(Cols, 1, PDF_FONT_NORMAL, "", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f,dlNo, pdfTblHdl);
            if (FromDate.Length != 0 || ToDate.Length != 0)
                Pdf_AddData(Cols, 1, PDF_FONT_NORMAL, "", PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, 5f, "Report Of : " + FromDate +
                                    (ToDate.Length != 0 && FromDate != ToDate ? "  To  " + ToDate : ""), pdfTblHdl);

            if (AddlnData != null && AddlnData.Length != 0)
            {
                for (i = 0; i < AddlnData.Length; i++)
                {
                    if (AddlnData[i] == null || AddlnData[i].Length == 0)
                        continue;
                    Pdf_AddData(AddlnDataColspan[i], 1, PDF_FONT_NORMAL, "", PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, 5f, AddlnData[i], pdfTblHdl);
                }
            }
            Pdf_AddNewLine(Cols, 1, "", pdfTblHdl);

            // Check Whether Total need to be shown
            TotReq = SubTotReq = 0;
            if (RptTotalCols != null)
                for (i = TotReq = 0; i < LColTot.Length; i++)
                    TotReq += (LColTot[i] == 1 ? 1 : 0);

            if (TotReq != 0)
            {
                TotReq = 1;
                for (i = TotColSpan = 0; i < LColTot.Length; i++)
                {
                    if (LColTot[i] == 1)
                        break;
                    TotColSpan++;
                }

                // Check Whether Sub Total need to be shown
                if (RptSubTotCols != null)
                {
                    for (i = SubTotReq = 0; i < LColSubTot.Length; i++)
                        SubTotReq += (LColSubTot[i] == 1 ? 1 : 0);
                    SubTotReq = SubTotReq == 0 ? 0 : 1;
                }
            }

            if (Row1MergeCols != null)      // Merger Row Header
            {
                for (i = 0; i < Row1MergeCols.Length; i++)
                    Pdf_AddData(Row1MergeCols[i] + AutoSlNo, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, 5f, Row1ColDesc[i], pdfTblHdl);
            }

            if (AutoSlNo == 1)
                Pdf_AddData(0, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, 5f, "#", pdfTblHdl);
            for (i = 0; i < dtRpt.Columns.Count; i++) // Column Heading
                Pdf_AddData(0, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, 5f, dtRpt.Columns[i].ColumnName, pdfTblHdl);

            // Data
            for (i = PrtSubTot = 0; i < dtRpt.Rows.Count; i++)
            {
                if (SubTotReq == 1 && i != 0)
                {
                    for (j = 0; j < TotColSpan; j++)
                        if (LColSubTot[j] == 1 && dtRpt.Rows[i - 1][j].ToString() != dtRpt.Rows[i][j].ToString()) // cols not equal, print sub total
                        {
                            PrtSubTot = 1;
                            break;
                        }

                    if (PrtSubTot == 1)
                    {
                        Pdf_AddData(TotColSpan + AutoSlNo, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f, "Sub-Total", pdfTblHdl);
                        for (j = TotColSpan; j < Cols; j++)
                            Pdf_AddData(AutoSlNo, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, ColDataType[i], 5f,
                                            LColSubTot[j] == 1 ? dSubTot[j].ToString() : "", pdfTblHdl);
                        Pdf_AddNewLine(Cols, 1, "1100", pdfTblHdl);
                        Array.Clear(dSubTot, 0, dTotal.Length);
                    }
                }

                PrtSubTot = 0;
                if (AutoSlNo == 1)
                    Pdf_AddData(0, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, RPTSTYLE_INT, 5f, (i + 1).ToString(), pdfTblHdl);
                for (j = 0; j < dtRpt.Columns.Count; j++)
                {
                    Pdf_AddData(0, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, ColDataType[j], 5f, dtRpt.Rows[i][j].ToString(), pdfTblHdl);
                    if (TotReq == 1 && LColTot[j] == 1)
                    {
                        dSubTot[j] += Convert.ToDouble(dtRpt.Rows[i][j]);
                        dTotal[j] += Convert.ToDouble(dtRpt.Rows[i][j]);
                    }
                }
            }   // end for

            if (TotReq == 1)
            {
                if (SubTotReq == 1)
                {
                    Pdf_AddData(TotColSpan + AutoSlNo, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f, "Sub-Total", pdfTblHdl);
                    for (j = TotColSpan; j < Cols; j++)
                        Pdf_AddData(0, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, ColDataType[j], 5f, LColSubTot[j] == 1 ? dSubTot[j].ToString() : "", pdfTblHdl);
                    Pdf_AddNewLine(Cols, 1, "1100", pdfTblHdl);
                }

                Pdf_AddData(TotColSpan + AutoSlNo, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f, "Total", pdfTblHdl);
                for (j = TotColSpan; j < Cols - AutoSlNo; j++)
                    Pdf_AddData(0, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_RIGHT, ColDataType[j], 5f, LColTot[j] == 1 ? dTotal[j].ToString() : "", pdfTblHdl);
            }

            Dcmnt.Add(pdfTblHdl);
            Dcmnt.Close();
            DBErrBuf = RptName;
            return 1;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "MC[43]: " + Ex.Message;
            return 0;
        }
    }


    public int GenerateReport(int DwnldFmt, int PdfLandscape, int ExlIsBorder, int AutoSlNo, string RptName, string Heading, int[] ColDataType, int[] RptSubTotCols,
                    int[] RptTotalCols, string FromDate, string ToDate, string[] AddlnData,int[]AddlnDataColspan, int[] Row1MergeCols, string[] Row1ColDesc, DataTable dtRpt, float[] colwdth)
    {
        /* Return --> 0 Error, 1 Success   [ sUtilErrBuf --> Error - Msg, Success - Report Full Path  ] 
         * IsLandscape --> If Pdf ==> 1 Landscape mode, 0 Portrait Mode
         * ExlIsBorder --> If Excel ==> Border Required for Col Header, Data, Totals [ 0 Not Req,  1 Req ]
         * AutoSlNo --> Show # in all Data Row Columns. [1 - Yes, 0 - NA]
         * RptName --> Name of the report. YYYYMMDDhhmmss + .xls/.pdf added with it
         * Heading --> Text to be printed as First Row
         * ColDataType --> Column's Data Type ==> Short String / String / Date [Left Align];   Int / Decimal [Right Align]
         * RptSubTotCols --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
         * RptTotalCols --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
         * From/To Date --> Both Avail  : Report Period XX to XX
         *                  Only ToDate : Report Of : XX
         *                  Both Empty  : No Date Desc
         * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
         * Row1MergeCols --> Apart from header, if addtiional header (above dt header) need to shown with merging cols count  / null --> Dont Merge
         * Row1ColDesc --> Apart from header, if addtional header (above dt header) need to shown with merging cols Desc / null --> Dont Merge
         */

        DBErrBuf = "";
        try
        {
            int ret = 0;
            string RptPath = "";

            RptName = RptName.Trim();
            if (RptName.Length == 0)
            {
                DBErrBuf = "MA[25]: Invalid Report Name";
                PrintLog("GenRpt", DBErrBuf);
                return 0;
            }

            RptName = "/Reports/" + RptName + (RptName.EndsWith("_") == true ? "" : "_") + System.DateTime.Now.ToString("yyyyMMddHHmmss") +
                                    (DwnldFmt == Globals.DWNLDFMT_EXCEL ? ".xlsx" : ".pdf");
            RptPath = HttpRuntime.AppDomainAppPath + RptName;
            if (File.Exists(RptPath))
                File.Delete(RptPath);

            AutoSlNo = AutoSlNo == 1 ? 1 : 0;
            if (DwnldFmt == Globals.DWNLDFMT_EXCEL)
                ret = GenDynamicExcel(RptName, Heading, ExlIsBorder, ColDataType, RptSubTotCols, RptTotalCols, FromDate, ToDate, AddlnData,
                                    Row1MergeCols, Row1ColDesc, dtRpt);
            else
                ret = GenDynamicPdf(RptName, Heading, PdfLandscape, AutoSlNo, ColDataType, RptSubTotCols, RptTotalCols, FromDate, ToDate, AddlnData,AddlnDataColspan,
                                    Row1MergeCols, Row1ColDesc, dtRpt,colwdth);
            if (ret == 1)
                DBErrBuf = RptName;
            return ret;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "MC[44]: " + Ex.Message;
            return 0;
        }
    }

    public int GenerateReport(int DwnldFmt, int PdfLandscape, int ExlIsBorder, int AutoSlNo, string RptName, string Heading, int[] ColDataType, int[] RptSubTotCols,
                    int[] RptTotalCols, string FromDate, string ToDate, string[] AddlnData, int[] Row1MergeCols, string[] Row1ColDesc, DataTable dtRpt)
    {
        /* Return --> 0 Error, 1 Success   [ sUtilErrBuf --> Error - Msg, Success - Report Full Path  ] 
         * IsLandscape --> If Pdf ==> 1 Landscape mode, 0 Portrait Mode
         * ExlIsBorder --> If Excel ==> Border Required for Col Header, Data, Totals [ 0 Not Req,  1 Req ]
         * AutoSlNo --> Show # in all Data Row Columns. [1 - Yes, 0 - NA]
         * RptName --> Name of the report. YYYYMMDDhhmmss + .xls/.pdf added with it
         * Heading --> Text to be printed as First Row
         * ColDataType --> Column's Data Type ==> Short String / String / Date [Left Align];   Int / Decimal [Right Align]
         * RptSubTotCols --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
         * RptTotalCols --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
         * From/To Date --> Both Avail  : Report Period XX to XX
         *                  Only ToDate : Report Of : XX
         *                  Both Empty  : No Date Desc
         * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
         * Row1MergeCols --> Apart from header, if addtiional header (above dt header) need to shown with merging cols count  / null --> Dont Merge
         * Row1ColDesc --> Apart from header, if addtional header (above dt header) need to shown with merging cols Desc / null --> Dont Merge
         */

        DBErrBuf = "";
        try
        {
            int ret = 0;
            string RptPath = "";

            RptName = RptName.Trim();
            if (RptName.Length == 0)
            {
                DBErrBuf = "MA[25]: Invalid Report Name";
                PrintLog("GenRpt", DBErrBuf);
                return 0;
            }

            RptName = "/Reports/" + RptName + (RptName.EndsWith("_") == true ? "" : "_") + System.DateTime.Now.ToString("yyyyMMddHHmmss") +
                                    (DwnldFmt == Globals.DWNLDFMT_EXCEL ? ".xlsx" : ".pdf");
            RptPath = HttpRuntime.AppDomainAppPath + RptName;
            if (File.Exists(RptPath))
                File.Delete(RptPath);

            AutoSlNo = AutoSlNo == 1 ? 1 : 0;
            if (DwnldFmt == Globals.DWNLDFMT_EXCEL)
                ret = GenDynamicExcel(RptPath, Heading, ExlIsBorder, AutoSlNo, ColDataType, RptSubTotCols, RptTotalCols, FromDate, ToDate, AddlnData,
                                    Row1MergeCols, Row1ColDesc, dtRpt);
            else
                ret = GenDynamicPdf(RptPath, Heading, PdfLandscape, AutoSlNo, ColDataType, RptSubTotCols, RptTotalCols, FromDate, ToDate, AddlnData,
                                    Row1MergeCols, Row1ColDesc, dtRpt);
            if (ret == 1)
                DBErrBuf = RptName;
            return ret;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "MC[44]: " + Ex.Message;
            PrintLog("GenRpt Exception", DBErrBuf);
            return 0;
        }
    }



    private int GenDynamicPdf(string RptName, string Heading, int IsLandscape, int AutoSlNo, int[] ColDataType, int[] RptSubTotCols, int[] RptTotalCols,
                                string FromDate, string ToDate, string[] AddlnData, int[] Row1MergeCols, string[] Row1ColDesc, DataTable dtRpt)
    {
        /* Return --> 0 Error, 1 Success   [ sUtilErrBuf --> Error - Msg, Success - Report Full Path  ] 
         * IsLandscape --> If Pdf ==> 1 Landscape mode, 0 Portrait Mode
         * AutoSlNo --> Show # in all Data Row Columns. [1 - Yes, 0 - NA]
         * RptName --> Name of the report. YYYYMMDDhhmmss + .xls/.pdf added with it
         * Heading --> Text to be printed as First Row
         * ColDataType --> Column's Data Type ==> Short String / String / Date [Left Align];   Int / Decimal [Right Align]
         * RptSubTotCols --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
         * RptTotalCols --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
         * From/To Date --> Both Avail  : Report Period XX to XX
         *                  Only ToDate : Report Of : XX
         *                  Both Empty  : No Date Desc
         * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
         * Row1MergeCols --> Apart from header, if addtiional header (above dt header) need to shown with merging cols count  / null --> Dont Merge
         * Row1ColDesc --> Apart from header, if addtional header (above dt header) need to shown with merging cols Desc / null --> Dont Merge
         */

        DBErrBuf = "";
        try
        {
            int i, j, k, Cols, TotColSpan, TotReq, SubTotReq, PrtSubTot;
            float[] PdfColWdt;
            int[] LColTot;
            int[] LColSubTot;
            double[] dTotal;
            double[] dSubTot;
            PdfPTable pdfTblHdl = new PdfPTable(dtRpt.Columns.Count + AutoSlNo);

            i = j = k = Cols = TotColSpan = TotReq = SubTotReq = PrtSubTot = 0;

            if (RptName.Length == 0)
            {
                DBErrBuf = "MA[21]: Invalid Report Path";
                PrintLog("GPdf", DBErrBuf);
                return 0;
            }

            PdfColWdt = Enumerable.Range(0, dtRpt.Columns.Count + AutoSlNo).Select(n => 8f).ToArray();
            LColTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            LColSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            dTotal = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0.00).ToArray();
            dSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0.00).ToArray();

            if (RptTotalCols != null)
                LColTot = RptTotalCols;
            if (RptSubTotCols != null)
                LColSubTot = RptSubTotCols;

            if (ColDataType.Length != dtRpt.Columns.Count || LColTot.Length != dtRpt.Columns.Count || LColSubTot.Length != dtRpt.Columns.Count)
            {
                DBErrBuf = "MA[22]: Invalid Report Columns";
                PrintLog("GPdf", DBErrBuf);
                return 0;
            }

            if (Row1MergeCols != null)
            {
                if (Row1MergeCols.Length != Row1ColDesc.Length || !(Row1MergeCols.Length <= ColDataType.Length) ||
                            !(Row1ColDesc.Length <= ColDataType.Length))
                {
                    DBErrBuf = "MA[23]: Invalid Report Columns";
                    PrintLog("GPdf", DBErrBuf);
                    return 0;
                }

                for (i = j = 0; i < Row1MergeCols.Length; i++)
                    j += Row1MergeCols[i];

                if (j != ColDataType.Length)
                {
                    DBErrBuf = "MA[24]: Invalid Report Columns";
                    PrintLog("GPdf", DBErrBuf);
                    return 0;
                }
            }

            k = 0;
            if (AutoSlNo == 1)
                PdfColWdt[k++] = 3f;
            for (i = 0; i < dtRpt.Columns.Count; i++, k++)
                PdfColWdt[k] = ColDataType[i] == RPTSTYLE_INT ? 3f : ColDataType[i] == RPTSTYLE_DECIMAL || ColDataType[i] == RPTSTYLE_DECIMAL_ROUND ? 4.5f :
                                    ColDataType[i] == RPTSTYLE_DATE || ColDataType[i] == RPTSTYLE_STRING_SHORT ? 5.5f : 9f;

            Cols = PdfColWdt.Length;
            pdfTblHdl.WidthPercentage = 100;
            pdfTblHdl.SetWidths(PdfColWdt);
            pdfTblHdl.HorizontalAlignment = 1;
            pdfTblHdl.SpacingBefore = 20f;
            pdfTblHdl.SpacingAfter = 30f;

            if (File.Exists(RptName) == true)
                File.Delete(RptName);

            Document Dcmnt = new Document(PageSize.A4, 25, 20, 25, 20);
            if (IsLandscape == 1)
                Dcmnt.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

            PdfWriter pdfwr = PdfWriter.GetInstance(Dcmnt, new FileStream(RptName, FileMode.Create));
            Dcmnt.Open();
            Pdf_AddData(Cols, 1, PDF_FONT_HIGH_BOLD, "", PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, 6f, Globals.gsAppTile, pdfTblHdl);

            if (Heading.Length != 0)
                Pdf_AddData(Cols, 1, PDF_FONT_HIGH, "", PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, 6f, Heading, pdfTblHdl);
            Pdf_AddNewLine(Cols, 1, "", pdfTblHdl);

            Pdf_AddData(Cols, 1, PDF_FONT_NORMAL, "", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f, "Report Generated On : " +
                        System.DateTime.Now.ToString("dd-MM-yyyy HH:mm"), pdfTblHdl);
            if (FromDate.Length != 0 || ToDate.Length != 0)
                Pdf_AddData(Cols, 1, PDF_FONT_NORMAL, "", PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, 5f, "Report Of : " + FromDate +
                                    (ToDate.Length != 0 && FromDate != ToDate ? "  To  " + ToDate : ""), pdfTblHdl);

            if (AddlnData != null && AddlnData.Length != 0)
            {
                for (i = 0; i < AddlnData.Length; i++)
                {
                    if (AddlnData[i] == null || AddlnData[i].Length == 0)
                        continue;
                    Pdf_AddData(Cols, 1, PDF_FONT_NORMAL, "", PdfPCell.ALIGN_LEFT, RPTSTYLE_STRING, 5f, AddlnData[i], pdfTblHdl);
                }
            }
            Pdf_AddNewLine(Cols, 1, "", pdfTblHdl);

            // Check Whether Total need to be shown
            TotReq = SubTotReq = 0;
            if (RptTotalCols != null)
                for (i = TotReq = 0; i < LColTot.Length; i++)
                    TotReq += (LColTot[i] == 1 ? 1 : 0);

            if (TotReq != 0)
            {
                TotReq = 1;
                for (i = TotColSpan = 0; i < LColTot.Length; i++)
                {
                    if (LColTot[i] == 1)
                        break;
                    TotColSpan++;
                }

                // Check Whether Sub Total need to be shown
                if (RptSubTotCols != null)
                {
                    for (i = SubTotReq = 0; i < LColSubTot.Length; i++)
                        SubTotReq += (LColSubTot[i] == 1 ? 1 : 0);
                    SubTotReq = SubTotReq == 0 ? 0 : 1;
                }
            }

            if (Row1MergeCols != null)      // Merger Row Header
            {
                for (i = 0; i < Row1MergeCols.Length; i++)
                    Pdf_AddData(Row1MergeCols[i] + AutoSlNo, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, 5f, Row1ColDesc[i], pdfTblHdl);
            }

            if (AutoSlNo == 1)
                Pdf_AddData(0, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, 5f, "#", pdfTblHdl);
            for (i = 0; i < dtRpt.Columns.Count; i++) // Column Heading
                Pdf_AddData(0, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_CENTER, RPTSTYLE_STRING, 5f, dtRpt.Columns[i].ColumnName, pdfTblHdl);

            // Data
            for (i = PrtSubTot = 0; i < dtRpt.Rows.Count; i++)
            {
                if (SubTotReq == 1 && i != 0)
                {
                    for (j = 0; j < TotColSpan; j++)
                        if (LColSubTot[j] == 1 && dtRpt.Rows[i - 1][j].ToString() != dtRpt.Rows[i][j].ToString()) // cols not equal, print sub total
                        {
                            PrtSubTot = 1;
                            break;
                        }

                    if (PrtSubTot == 1)
                    {
                        Pdf_AddData(TotColSpan + AutoSlNo, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f, "Sub-Total", pdfTblHdl);
                        for (j = TotColSpan; j < Cols; j++)
                            Pdf_AddData(AutoSlNo, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, ColDataType[i], 5f,
                                            LColSubTot[j] == 1 ? dSubTot[j].ToString() : "", pdfTblHdl);
                        Pdf_AddNewLine(Cols, 1, "1100", pdfTblHdl);
                        Array.Clear(dSubTot, 0, dTotal.Length);
                    }
                }

                PrtSubTot = 0;
                if (AutoSlNo == 1)
                    Pdf_AddData(0, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, RPTSTYLE_INT, 5f, (i + 1).ToString(), pdfTblHdl);
                for (j = 0; j < dtRpt.Columns.Count; j++)
                {
                    Pdf_AddData(0, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, ColDataType[j], 5f, dtRpt.Rows[i][j].ToString(), pdfTblHdl);
                    if (TotReq == 1 && LColTot[j] == 1)
                    {
                        dSubTot[j] += Convert.ToDouble(dtRpt.Rows[i][j]);
                        dTotal[j] += Convert.ToDouble(dtRpt.Rows[i][j]);
                    }
                }
            }   // end for

            if (TotReq == 1)
            {
                if (SubTotReq == 1)
                {
                    Pdf_AddData(TotColSpan + AutoSlNo, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f, "Sub-Total", pdfTblHdl);
                    for (j = TotColSpan; j < Cols; j++)
                        Pdf_AddData(0, 1, PDF_FONT_NORMAL, "1111", PdfPCell.ALIGN_RIGHT, ColDataType[j], 5f, LColSubTot[j] == 1 ? dSubTot[j].ToString() : "", pdfTblHdl);
                    Pdf_AddNewLine(Cols, 1, "1100", pdfTblHdl);
                }

                Pdf_AddData(TotColSpan + AutoSlNo, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_RIGHT, RPTSTYLE_STRING, 5f, "Total", pdfTblHdl);
                for (j = TotColSpan; j < Cols - AutoSlNo; j++)
                    Pdf_AddData(0, 1, PDF_FONT_NORMAL_BOLD, "1111", PdfPCell.ALIGN_RIGHT, ColDataType[j], 5f, LColTot[j] == 1 ? dTotal[j].ToString() : "", pdfTblHdl);
            }

            Dcmnt.Add(pdfTblHdl);
            Dcmnt.Close();
            DBErrBuf = RptName;
            return 1;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "MC[43]: " + Ex.Message;
            PrintLog("GPdf Exception", DBErrBuf);
            return 0;
        }
    }


    private int GenDynamicExcel(string RptName, string Heading, int BorderReq, int AutoSlNo, int[] ColDataType, int[] RptSubTotCols, int[] RptTotalCols,
                            string FromDate, string ToDate, string[] AddlnData, int[] Row1MergeCols, string[] Row1ColDesc, DataTable dtRpt)
    {
        /* Return --> 0 Error, 1 Success  [  sUtilErrBuf --> Error - Msg, Success - Report Full Path  ]
         * BorderReq --> If Excel ==> Border Required for Col Header, Data, Totals [ 0 Not Req,  1 Req ]
         * AutoSlNo --> Show # in all Data Row Columns. [1 - Yes, 0 - NA]
         * RptName --> Name of the report. YYYYMMDDhhmmss + .xls/.pdf added with it
         * Heading --> Text to be printed as First Row
         * ColDataType --> Column's Data Type ==> Short String / String / Date [Left Align];   Int / Decimal [Right Align]
         * RptSubTotCols --> Based on these columns, calculate sub total. [0 Not Req, 1 Group by this Col] / null --> Dont calculate
         * RptTotalCols --> Columns for which Total Need to be calculated [0 Not Req, 1 Total Req]   / null --> Dont Calculate
         * From/To Date --> Both Avail  : Report Period XX to XX
         *                  Only ToDate : Report Of : XX
         *                  Both Empty  : No Date Desc
         * AddlnData --> Additional Datas need to printed in pdf. Null - No Data Avail
         * Row1MergeCols --> Apart from header, if addtiional header (above dt header) need to shown with merging cols count  / null --> Dont Merge
         * Row1ColDesc --> Apart from header, if addtional header (above dt header) need to shown with merging cols Desc / null --> Dont Merge
         */

        DBErrBuf = "";
        try
        {
            int i, col, row, TotStCol, TotStRow, SubTotStRow, PrtSubTot;
            int[] LColTot = new int[ColDataType.Length];
            int[] LColSubTot = new int[ColDataType.Length];

            i = col = row = TotStCol = TotStRow = SubTotStRow = PrtSubTot = 0;
            Array.Clear(LColTot, 0, LColTot.Length);
            Array.Clear(LColSubTot, 0, LColSubTot.Length);

            if (RptName.Length == 0)
            {
                DBErrBuf = "MA[17]: Invalid File Name";
                return 0;
            }

            LColTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            LColSubTot = Enumerable.Range(0, dtRpt.Columns.Count).Select(n => 0).ToArray();
            if (RptTotalCols != null)
                LColTot = RptTotalCols;
            if (RptSubTotCols != null)
                LColSubTot = RptSubTotCols;

            if (ColDataType.Length != dtRpt.Columns.Count || ColDataType.Length != LColTot.Length || ColDataType.Length != LColSubTot.Length)
            {
                DBErrBuf = "MA[18]: Invalid Report Columns";
                return 0;
            }

            if (Row1MergeCols != null)
            {
                if (Row1MergeCols.Length != Row1ColDesc.Length || !(Row1MergeCols.Length <= ColDataType.Length) ||
                            !(Row1ColDesc.Length <= ColDataType.Length))
                {
                    DBErrBuf = "MA[19]: Invalid Report Columns";
                    return 0;
                }

                for (i = row = 0; i < Row1MergeCols.Length; i++)
                    row += Row1MergeCols[i];

                if (row != ColDataType.Length)
                {
                    DBErrBuf = "MA[20]: Invalid Report Columns";
                    return 0;
                }
            }

            row = col = 1;
            using (ExcelPackage ExlPack = new ExcelPackage())
            {
                //Here setting some document properties
                ExlPack.Workbook.Properties.Author = "Origin Technology Associates";
                ExlPack.Workbook.Properties.Title = Globals.gsAppTile;

                //Create a sheet
                ExlPack.Workbook.Worksheets.Add(Heading);
                ExcelWorksheet ws = ExlPack.Workbook.Worksheets[0];
                ws.Name = "Sheet";     //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                // Aligning Header Details
                if (Globals.gsAppTile.Length != 0)
                    Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count + AutoSlNo, true, 18, ExcelHorizontalAlignment.Center, 0, Color.Empty, 0, Globals.gsAppTile, "");
                if (Heading.Length != 0)
                    Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count + AutoSlNo, true, 0, ExcelHorizontalAlignment.Center, 0, Color.Empty, 0, Heading, "");
                row++;
                Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count + AutoSlNo, true, 0, ExcelHorizontalAlignment.Right, 0, Color.Empty, 0,
                            "Generated On:" + System.DateTime.Now.ToString("dd-MM-yyyy HH:mm"), "");
                if (FromDate.Length != 0 || ToDate.Length != 0)
                    Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count + AutoSlNo, true, 0, ExcelHorizontalAlignment.Left, 0, Color.Empty,
                                0, "Report of : " + FromDate + (ToDate.Length != 0 && FromDate != ToDate ? "  To  " + ToDate : ""), "");
                if (AddlnData != null && AddlnData.Length != 0)
                {
                    row++;
                    for (i = 0; i < AddlnData.Length; i++)
                        if (AddlnData[i] != null && AddlnData[i].Length != 0)
                            Excel_AddData(ws, 1, ref row, 1, dtRpt.Columns.Count + AutoSlNo, true, 0, ExcelHorizontalAlignment.Left, 0, Color.Empty, 0, AddlnData[i], "");
                }

                if (Row1MergeCols != null)
                {
                    row++;
                    for (i = 0, col = AutoSlNo + 1; i < Row1MergeCols.Length; i++)
                    {
                        Excel_AddData(ws, 0, ref row, col, col + AutoSlNo + Row1MergeCols[i] - 1, true, 0, ExcelHorizontalAlignment.Center, BorderReq,
                                Color.LightGray, RPTSTYLE_STRING, Row1ColDesc[i], "");
                        col += Row1MergeCols[i];
                    }
                }

                row++;
                if (AutoSlNo == 1)
                    Excel_AddData(ws, 0, ref row, 1, 1, true, 0, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray, RPTSTYLE_STRING, "#", "");
                for (col = 0; col < dtRpt.Columns.Count; col++)
                    Excel_AddData(ws, 0, ref row, col + AutoSlNo + 1, col + AutoSlNo + 1, true, 0, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray,
                                RPTSTYLE_STRING, dtRpt.Columns[col].ColumnName, "");
                row++;

                // Check Whether Total need to be shown
                TotStCol = TotStRow = SubTotStRow = 0;
                if (RptTotalCols != null)
                {
                    for (i = col = 0; i < LColTot.Length; i++)
                        col += (LColTot[i] == 1 ? 1 : 0);
                    if (col != 0)
                    {
                        TotStRow = row;
                        for (i = TotStCol = 0; i < LColTot.Length; i++)
                        {
                            if (LColTot[i] == 1)
                                break;
                            TotStCol++;
                        }

                        // Check Whether Sub Total need to be shown
                        if (RptSubTotCols != null)
                        {
                            for (i = col = 0; i < LColSubTot.Length; i++)
                                col += (LColSubTot[i] == 1 ? 1 : 0);
                            SubTotStRow = col == 0 ? 0 : row;
                        }
                    }
                }

                for (i = 0; i < dtRpt.Rows.Count; i++) // Adding Data into rows
                {
                    if (SubTotStRow != 0 && i != 0)
                    {
                        for (col = 0; col < TotStCol; col++)
                            if (LColSubTot[col] == 1 && dtRpt.Rows[i - 1][col].ToString() != dtRpt.Rows[i][col].ToString()) // cols not equal, print sub total
                            {
                                PrtSubTot = 1;
                                break;
                            }

                        if (PrtSubTot == 1)
                        {
                            Excel_AddData(ws, 0, ref row, 1, TotStCol + AutoSlNo, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray,
                                        0, "Sub-Total :", "");
                            for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                                Excel_AddData(ws, 0, ref row, col + AutoSlNo + 1, col + AutoSlNo + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq,
                                        Color.LightGray, ColDataType[col], "", LColSubTot[col] == 0 ? "" :
                                        "SUBTOTAL(9," + ws.Cells[SubTotStRow, col + 1].Address + ":" + ws.Cells[row - 1, col + 1].Address + ")");
                            row += 2;
                            SubTotStRow = row;
                        }
                    }

                    PrtSubTot = 0;
                    if (AutoSlNo == 1)
                        Excel_AddData(ws, 0, ref row, 1, 1, false, 0, ExcelHorizontalAlignment.General, BorderReq, Color.Empty, RPTSTYLE_INT, (i + 1).ToString(), "");
                    for (col = 0; col < dtRpt.Columns.Count; col++)
                        Excel_AddData(ws, 0, ref row, col + AutoSlNo + 1, col + AutoSlNo + 1, false, 0, ExcelHorizontalAlignment.General, BorderReq, Color.Empty,
                                ColDataType[col], dtRpt.Rows[i][col].ToString(), "");
                    row++;
                }

                if (TotStRow != 0)
                {
                    if (SubTotStRow != 0)
                    {
                        Excel_AddData(ws, 0, ref row, 1, TotStCol + AutoSlNo, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.LightGray,
                                              0, "Sub-Total :", "");
                        for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                            Excel_AddData(ws, 0, ref row, col + AutoSlNo + 1, col + AutoSlNo + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq,
                                    Color.LightGray, ColDataType[col], "", LColSubTot[col] == 0 ? "" :
                                    "SUBTOTAL(9," + ws.Cells[SubTotStRow, col + AutoSlNo + 1].Address + ":" + ws.Cells[row - 1, col + AutoSlNo + 1].Address + ")");
                        row += 2;
                    }

                    Excel_AddData(ws, 0, ref row, 1, TotStCol + AutoSlNo, true, 13, ExcelHorizontalAlignment.Center, BorderReq, Color.FromArgb(173, 173, 173),
                                0, "Total :", "");
                    for (col = TotStCol; col < dtRpt.Columns.Count; col++)
                        Excel_AddData(ws, 0, ref row, col + AutoSlNo + 1, col + AutoSlNo + 1, true, 13, ExcelHorizontalAlignment.Right, BorderReq,
                                Color.FromArgb(173, 173, 173), ColDataType[col], "", LColTot[col] == 0 ? "" :
                                "SUBTOTAL(9," + ws.Cells[TotStRow, col + AutoSlNo + 1].Address + ":" + ws.Cells[row - 1, col + AutoSlNo + 1].Address + ")");
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                Byte[] bin = ExlPack.GetAsByteArray();
                File.WriteAllBytes(RptName, bin);
                DBErrBuf = RptName;
                return 1;
            }
        }
        catch (Exception Ex)
        {
            DBErrBuf = "MC[42]: " + Ex.Message;
            PrintLog("GDExl Exception", DBErrBuf);
            return 0;
        }
    }



    public int MergePdf(string docname)
    {
        string SourcePdfPath = HttpContext.Current.Server.MapPath("../Reports");
        string outputFileName = "../Reports/" + docname + "_final.pdf";
        string outputPath = HttpContext.Current.Server.MapPath(outputFileName);
        if (File.Exists(outputPath))
            File.Delete(outputPath);

        string[] filenames = System.IO.Directory.GetFiles(SourcePdfPath, docname+"*");
        Document doc = new Document();
        PdfCopy writer = new PdfCopy(doc, new FileStream(outputPath, FileMode.Create));
        if (writer==null)
        {
            return 0;
        }
        doc.Open();
        foreach (string filename in filenames)
        {
            PdfReader reader = new PdfReader(filename);
            reader.ConsolidateNamedDestinations();
            for (int i = 1; i<=reader.NumberOfPages; i++)
            {
                PdfImportedPage page = writer.GetImportedPage(reader, i);
                writer.AddPage(page);
            }
            reader.Close();
            //File.Delete(SourcePdfPath + filename);
        }
        writer.Close();
        doc.Close();
        DBErrBuf = outputFileName;
        return 1;
    }

    public List<T> ConvertToList<T>(DataTable dt)
    {
        try
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();
            var objectProperties = typeof(T).GetProperties(flags);
            var targetList = dt.AsEnumerable().Select(dataRow =>
            {
                var instanceOfT = Activator.CreateInstance<T>();

                foreach (var properties in objectProperties.Where(properties => columnNames.Contains(properties.Name) && dataRow[properties.Name] != DBNull.Value))
                    properties.SetValue(instanceOfT, dataRow[properties.Name], null);
                return instanceOfT;
            }).ToList();

            return targetList;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[17]: " + Ex.Message.ToString();
            PrintLog("MasterLogic Qry : ", DBErrBuf);
            return null;
        }
    }

    public DataTable ToDataTable<T>(List<T> items)
    {
        DataTable dataTable = new DataTable(typeof(T).Name);

        //Get all the properties
        PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo prop in Props)
        {
            //Setting column names as Property names
            dataTable.Columns.Add(prop.Name);
        }
        foreach (T item in items)
        {
            var values = new object[Props.Length];
            for (int i = 0; i < Props.Length; i++)
            {
                //inserting property values to datatable rows
                values[i] = Props[i].GetValue(item, null);
            }
            dataTable.Rows.Add(values);
        }
        //put a breakpoint here and check datatable
        return dataTable;
    }
}
