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
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Windows;
using Naga;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO.Ports;

public class MasterLogic
{
    MySqlConnection Con;

    ILog logwriter = LogManager.GetLogger("FILE");
    SerialPort serial = new SerialPort();
    public string DBErrBuf = "";

    // ******************************* DB Related Functions *********************
    public MasterLogic()
    {
        log4net.Config.XmlConfigurator.Configure();
        string[] StrCon = ConfigurationManager.ConnectionStrings["MySqlCon"].ToString().Split(new char[] { ';', },
                                    StringSplitOptions.RemoveEmptyEntries);
        string UserId = StrCon[0].Substring(StrCon[0].IndexOf("=") + 1).Trim();
        string Pwd = StrCon[1].Substring(StrCon[1].IndexOf("=") + 1).Trim();
        UserId = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(UserId));
        Pwd = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(Pwd));
        string tmp = "Data Source= " + Globals.IP + ";Database=" + Globals.Database + ";User=" + UserId + ";Password=" + Pwd + ";";
        Con = new MySqlConnection(tmp);
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

    public int ChkDateRange(int Flage, string FromDate, string ToDate, Label lblMsg)
    {
        // Flage 1 - Chk FromDate <= CurrentDate & ToDate ==""
        // Flage 2 - Chk FromDate <= CurrentDate & ToDate <= CurrentDate & FromDate <= ToDate

        try
        {
            int ret = 0;
            string buf = DateTime.Now.ToString("dd-MM-yyyy");
            DateTime dtCtDate = DateTime.Parse(buf.Substring(6, 4) + buf.Substring(2, 4) + buf.Substring(0, 2));

            if (FromDate == "" || FromDate == string.Empty || FromDate.Length != 10)
            {
                lblMsg.Content = "Select Valid Date";
                return 0;
            }

            DateTime dtFromDate = DateTime.Parse(FromDate.Substring(6, 4) + FromDate.Substring(2, 4) + FromDate.Substring(0, 2));
            ret = dtCtDate.CompareTo(dtFromDate);
            if (ret <= -1)
            {
                lblMsg.Content = "Select Valid Date";
                return 0;
            }

            if (Flage == 1)
                return 1;

            if (ToDate == "" || ToDate == string.Empty || ToDate.Length != 10)
            {
                lblMsg.Content = "Select Valid To Date";
                return 0;
            }

            DateTime dtToDate = DateTime.Parse(ToDate.Substring(6, 4) + ToDate.Substring(2, 4) + ToDate.Substring(0, 2));
            ret = dtCtDate.CompareTo(dtToDate);
            if (ret <= -1)
            {
                lblMsg.Content = "ToDate Cannot be higher than Current Date";
                return 0;
            }

            ret = dtToDate.CompareTo(dtFromDate);
            if (ret <= -1)
            {
                lblMsg.Content = "From Date Cannot be higher than ToDate";
                return 0;
            }

            return 1;
        }

        catch (Exception Ex)
        {
            lblMsg.Content = Ex.Message.ToString();
            return 0;
        }
    }


    public DataTable ValidateLogin(string strLoginId, string strPassword)
    {
        DataSet dsLogin = new DataSet();
        try
        {
            Con.Open();
            MySqlCommand cmd = new MySqlCommand(" Select * from systemuser_master where UserId = '" + strLoginId.ToUpper() + "' and UserPWD = '" +
                                                strPassword + "' and Status  in (1,2) AND Find_in_set('" + Globals.System + "',Systems) and EmpId <> 0", Con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dsLogin);
        }

        finally
        {
            Con.Close();
        }
        return dsLogin.Tables[0];
    }

    public int GetCount(string sQry, ref Int64 iDstVal)
    {
        try
        {
            iDstVal = 0;
            Con.Open();
            MySqlCommand cmd = new MySqlCommand(sQry, Con);
            iDstVal = Convert.ToInt32(cmd.ExecuteScalar());
            return 1;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[4]: " + Ex.Message.ToString();
            return -1;
        }
        finally
        {
            Con.Close();
        }
    }

    public Int64 InsertMasterUpload(string sMaxIdQry, string[] sQryHead, string[] sQryTail, int sQryCount, ref string Id)
    {
        Int64 ret = 0;
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
                cmdQry = new MySqlCommand(qry, Con, Tran);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = 1;
            Tran.Commit();
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[8]: " + Ex.Message.ToString();
            Tran.Rollback();
            ret = -1;

        }
        finally
        {
            Con.Close();
        }

        return ret;
    }

    public Int64 Uploadmaster(string sMaxIdQry, string[] sQryHead, string[] sQryTail, string[] sQryStrHead, string[] sQryStrTail, int sQryCount)
    {
        Int64 ret = 0;

        Con.Open();
        MySqlTransaction Tran = Con.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            string Id = "", qry = "";
            MySqlCommand cmdQry;
            MySqlCommand cmdId;


            for (int i = 0; i < sQryCount; i++)
            {
                logwriter.Debug("MasterLogic Qry : " + sMaxIdQry);
                cmdId = new MySqlCommand(sMaxIdQry, Con);
                Id = Convert.ToString(cmdId.ExecuteScalar());
                if (Id == "")
                {
                    DBErrBuf = "DC[15,1]: Failed to Fetch Id";
                    logwriter.Debug("MasterLogic Error: " + DBErrBuf);
                    Tran.Rollback();
                    return -1;
                }
                qry = "";
                qry = sQryHead[i] + Id + sQryTail[i];
                if (qry == "")
                    continue;
                logwriter.Debug("MasterLogic Qry : " + qry);
                cmdQry = new MySqlCommand(qry, Con);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();

                qry = "";
                qry = sQryStrHead[i] + Id + sQryStrTail[i];
                if (qry == "")
                    continue;
                logwriter.Debug("MasterLogic Qry : " + qry);
                cmdQry = new MySqlCommand(qry, Con);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = Convert.ToInt64(Id);
            Tran.Commit();
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[15]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            Tran.Rollback();
            ret = -1;

        }
        finally
        {
            Con.Close();
        }
        return ret;
    }

    public string FDBS(string SrcStr)
    {
        string DstBuf = "";

        DstBuf = SrcStr.Replace("'", "''");
        return DstBuf;
    }

    public int ExecMultipleQry(int Cnt, string[] sQry)
    {
        int AffRows = 0, i = 0;
        
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
                AffRows += Convert.ToInt16(cmd.ExecuteNonQuery());
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
            ret = Convert.ToInt64(cmd.ExecuteScalar());
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

    public int GetJobDt(string strQuery, ref DataTable dt)
    {
        int ret = -1;
        try
        {
            Con.Open();
            MySqlCommand cmd = new MySqlCommand(strQuery, Con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            if (dt.Rows.Count > 0)
                ret = 1;
            else
                ret = 0;
        }
        catch (Exception Ex)
        {
            DBErrBuf = Ex.Message.ToString() + "Query : " + strQuery.ToString();
            ret = -1;
        }
        finally
        {

            Con.Close();
        }
        return ret;
    }

    public Int64 InsertMaster(string sMaxIdQry, string[] sQryHead, string[] sQryTail,int sQryCount)
    {
        Int64 ret = 0;

        Con.Open();
        MySqlTransaction Tran = Con.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            string Id = "", qry = "";
            MySqlCommand cmdQry;
            MySqlCommand cmdId = new MySqlCommand(sMaxIdQry, Con, Tran);
            logwriter.Debug("MasterLogic Qry : " + sMaxIdQry);
            Id = Convert.ToString(cmdId.ExecuteScalar());
            cmdId.Dispose();
            if (Id == "")
            {
                DBErrBuf = "DC[8,1]: Failed to Fetch Id";
                logwriter.Debug("MasterLogic Error:" + DBErrBuf);
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
                logwriter.Debug("MasterLogic Qry : " + qry);
                cmdQry = new MySqlCommand(qry, Con, Tran);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = 1;
            Tran.Commit();
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[8]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            Tran.Rollback();
            ret = -1;

        }
        finally
        {
            Con.Close();
        }

        return ret;
    }
    
    //*********************************************************Session Login****************************************************

    public Int64 ChkSessionLogin(string UserId, string SessionId)
    {
        try
        {
            Con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(1) FROM Logins WHERE UserId='" + UserId + "' AND SessionId='" + SessionId + "' AND Status=1", Con);
            int ret = Convert.ToInt32(cmd.ExecuteScalar());
            Con.Close();
            return ret;
        }
        catch (Exception Ex)
        {
            Con.Close();
            DBErrBuf = "DC[9]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            return -1;
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
            DBErrBuf = "DC[11]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
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
            DBErrBuf = "DC[10]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            return -1;
        }
    }

    public Int64 CheckShiftOpen(string System)
    {
        Int64 ret;
        try 
        {
            Con.Open();
            logwriter.Debug(" MasterLogic Qry : Select Count(1) from shift_master Where ShiftStatus=1 AND SystemId='" + System
                            + "' AND UnitId='" + Globals.Unit + "' AND PlantCode='" + Globals.Plant + "'");
            MySqlCommand cmd = new MySqlCommand(" Select Count(1) from shift_master Where ShiftStatus=1 AND SystemId='" +
                                     System + "' AND UnitId='" + Globals.Unit + "' AND PlantCode='" + Globals.Plant + "'", Con);
            ret = Convert.ToInt64(cmd.ExecuteScalar());
            Con.Close();
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[14]: " + Ex.Message.ToString();
            Con.Close();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            return -1;
        }
        return ret;
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

    public Int64 Uploadmaster(string sMaxIdQry, string[] sQryHead, string[] sQryTail, int sQryCount)
    {
        Int64 ret = 0;

        Con.Open();
        MySqlTransaction Tran = Con.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            string Id = "", qry = "";
            MySqlCommand cmdQry;
            MySqlCommand cmdId;


            for (int i = 0; i < sQryCount; i++)
            {
                logwriter.Debug("MasterLogic Qry : " + sMaxIdQry);
                cmdId = new MySqlCommand(sMaxIdQry, Con);
                Id = Convert.ToString(cmdId.ExecuteScalar());
                if (Id == "")
                {
                    DBErrBuf = "DC[15,1]: Failed to Fetch Id";
                    logwriter.Debug("MasterLogic Error: " + DBErrBuf);
                    Tran.Rollback();
                    return -1;
                }
                qry = "";
                qry = sQryHead[i] + Id + sQryTail[i];
                if (qry == "")
                    continue;
                logwriter.Debug("MasterLogic Qry : " + qry);
                cmdQry = new MySqlCommand(qry, Con);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = Convert.ToInt64(Id);
            Tran.Commit();
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[15]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            Tran.Rollback();
            ret = -1;

        }
        finally
        {
            Con.Close();
        }
        return ret;
    }

    public Int64 InsertTransaction(string sMaxIdQry, string sStockCheckQry, string sQryHead, string sQryTail, string[] sQryDetails, int sQryCount)
    {
        Int64 ret = 0;

        Con.Open();
        MySqlTransaction Tran = Con.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            string Id = "", qry = "";
            MySqlCommand cmdQry, cmdId;

            if (sMaxIdQry.Trim() != "")
            {
                cmdId = new MySqlCommand(sMaxIdQry, Con);
                logwriter.Debug("MasterLogic Qry : " + sMaxIdQry);
                Id = Convert.ToString(cmdId.ExecuteScalar());
                cmdId.Dispose();
                if (Id == "")
                {
                    DBErrBuf = "DC[16,1]: Failed to Fetch Id";
                    logwriter.Debug("MasterLogic Error:" + DBErrBuf);

                    Tran.Rollback();
                    return -1;
                }
            }
            if (sStockCheckQry.Trim() != "")
            {
                cmdId = new MySqlCommand(sStockCheckQry, Con);
                ret = Convert.ToInt16(cmdId.ExecuteScalar().ToString());
                cmdId.Dispose();
                if (ret != 0)
                {
                    DBErrBuf = "DC[16,2]: Transaction Stock Not Available";
                    logwriter.Debug("MasterLogic Error:" + DBErrBuf);
                    Tran.Rollback();
                    return -1;
                }
            }
            
            if (sQryHead.Trim() != "" && sQryTail.Trim() != "" && Id.Trim() != "")
            {
                qry = "";
                qry = sQryHead + Id + sQryTail;
                cmdId = new MySqlCommand(qry, Con);
                ret = cmdId.ExecuteNonQuery();
                cmdId.Dispose();
                if (ret <= 0)
                {
                    DBErrBuf = "DC[16,3]: Failed to Insert Master Qry";
                    logwriter.Debug("MasterLogic Error:" + DBErrBuf);
                    Tran.Rollback();
                    return -1;
                }
            }
            for (int i = 0; i < sQryCount; i++)
            {
                qry = sQryDetails[i].ToString();
                if (qry == "")
                    continue;
                logwriter.Debug("MasterLogic Qry : " + qry);
                cmdQry = new MySqlCommand(qry, Con);
                ret += cmdQry.ExecuteNonQuery();
                cmdQry.Dispose();
            }
            ret = 1;
            Tran.Commit();
        }

        catch (Exception Ex)
        {
            DBErrBuf = "DC[16]: " + Ex.Message.ToString();
            logwriter.Debug("MasterLogic Error: " + DBErrBuf);
            Tran.Rollback();
            ret = -1;

        }
        finally
        {
            Con.Close();
        }

        return ret;
    }

}
