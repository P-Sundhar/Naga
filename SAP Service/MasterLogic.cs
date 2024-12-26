using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
using System.IO;
using log4net;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;

public class MasterLogic
{
    MySqlConnection Con;

    ILog logwriter = LogManager.GetLogger("FILE");
    public string DBErrBuf = "";

    // ******************************* DB Related Functions *********************
    public MasterLogic()
    {
        string[] StrCon = ConfigurationManager.AppSettings["MySqlCon"].ToString().Split(new char[] { ';', }, StringSplitOptions.RemoveEmptyEntries);
        string UserId = StrCon[2].Substring(StrCon[2].IndexOf("=") + 1).Trim();
        string Password = StrCon[3].Substring(StrCon[3].IndexOf("=") + 1).Trim();
        string Pooling = StrCon[4].Substring(StrCon[4].IndexOf("=") + 1).Trim();

        string buf = StrCon[0] + ";" + StrCon[1] + ";User=" + UserId + ";Password=" + Password + ";Pooling=" + Pooling + ""; 
        Con = new MySqlConnection(buf);
    }

    public void PrintLog(string FnName, string Data)
    {
        try
        {
            logwriter.Debug(FnName + " : " + Data);
        }

        catch { }
    }

    // Encryption / Decryption Process
    public string EncodeData(string Data)
    {
        try
        {
            string buf = "";

            buf = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Data));
            return buf;
        }

        catch (Exception ex)
        {
            DBErrBuf = ex.Message.ToString();
            return "";
        }
    }

    public string DecodeData(string Data)
    {
        try
        {
            string buf = "";

            buf = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(Data));
            return buf;
        }

        catch (Exception ex)
        {
            DBErrBuf = ex.Message.ToString();
            return "";
        }
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
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            return 0;
        }
        finally
        {
            Con.Close();
        }
    }

    public Int64 ExecMultipleQry(int Cnt, string[] sQry)
    {
        Int64 AffRows = 0;
        int  i = 0;
        
        DBErrBuf = "";

        Con.Open();
        MySqlTransaction objMysT = Con.BeginTransaction();

        try
        {
            MySqlCommand cmd;
            for (i = 0; i < Cnt; i++)
            {
                if (sQry[i] == string.Empty || sQry[i] == "")
                    continue;
                logwriter.Debug("MasterLogic Qry : " + sQry[i]);
                cmd = new MySqlCommand(sQry[i], Con,objMysT);
                AffRows += Convert.ToInt64(cmd.ExecuteNonQuery());
                cmd.Dispose();
            }
            objMysT.Commit();
        }

        catch(Exception Ex)
        {
            objMysT.Rollback();
            AffRows = -1;
            DBErrBuf = "DC[2]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
        }
        finally
        {
            Con.Close();
        }
        return AffRows;
    }

    public int ExecScalar(string sQry, ref string buf)
    {
        DBErrBuf = "";
        try
        {
            buf = "";
            Con.Open();
            logwriter.Debug("MasterLogic Qry : " + sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            buf = cmd.ExecuteScalar().ToString();
            if (buf.Length == 0)
                return 0;

            return 1;
        }
        catch(Exception Ex)
        {
            DBErrBuf = "DC[3]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            return -1;
        }
        finally
        {
            Con.Close();
        }
    }

    public int GetId(string sQry, ref Int64 iDstVal)
    { 
        DBErrBuf = "";

        try
        {
            iDstVal = 0;
            Con.Open();
            logwriter.Debug("MasterLogic Qry : " + sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            iDstVal = Convert.ToInt32(cmd.ExecuteScalar());
            return 1;
        }
        catch(Exception Ex)
        {
            DBErrBuf = "DC[4]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
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
            Con.Open();
            logwriter.Debug("MasterLogic Qry : SELECT COUNT(1) FROM " + sQry);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(1) FROM " + sQry, Con);
            ret = Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[5]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            ret = -1;
        }
        finally
        {
            Con.Close();
        }
        return ret;
    }

    public int ExecQry(string Qry)
    {
        int ret = 0;

        DBErrBuf = "";

        try
        {
            Con.Open();
            logwriter.Debug("MasterLogic Qry : " + Qry);
            MySqlCommand cmd = new MySqlCommand(Qry, Con);
            ret = cmd.ExecuteNonQuery();
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[6]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            ret = -1;
        }
        finally
        {
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
            Con.Open();
            logwriter.Debug("MasterLogic Qry : " + sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            cmd.CommandTimeout = 180;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[7]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            dt = null;
        }
        finally
        {
            Con.Close();
        }
        return dt;
    }

    public DataSet GetDataSet(string sQry)
    {
        DataSet ds = new DataSet();

        DBErrBuf = "";
        try
        {
            Con.Open();
            logwriter.Debug("MasterLogic Qry : " + sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(ds, "LoadDataBinding");
            return ds;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[7]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            ds = null;
        }
        finally
        {
            Con.Close();
        }
        return ds;
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
            DBErrBuf = "DC[12]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
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
            DBErrBuf = "DC[14]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            return "";
        }
    }

    public string FDBS(string SrcStr)
    {
        string DstBuf = "";
       
        DstBuf = SrcStr.Replace("'", "''");
        DstBuf = SrcStr.Replace( "’", "''");
        return DstBuf;
    }


   
}
