using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using MySql.Data.MySqlClient;
using log4net;
using System.Text;
using System.Web.UI;
using System.Reflection;

/// <summary>
/// Summary description for MasterLogic
/// </summary>
public class MasterLogic
{
    ILog logger = LogManager.GetLogger("File");
    MySqlConnection con;
    public string DBErrBuf = "";
    
    public MasterLogic()
    {
        con = new MySqlConnection(ConfigurationManager.AppSettings["Mysqlconn"].ToString());
    }
    
    public void PrintLog(string FnName, string Data)
    {
        try
        {
            logger.Debug(FnName + " : " + Data);
        }

        catch { }
    }

    public int ConnectionCheck()
    {
        try
        {
            con.Open();
            return 1;
        }
        catch (Exception ex)
        {
            logger.Debug("ConnectionCheck ERROR : " + ex.Message.ToString());
            return 0;
        }
        finally
        {
            con.Close();
        }
    }
   
    public Int64 GetTableCount(string sQry)
    {
        Int64 ret = 0;
   
        try
        {
            con.Open();
            logger.Debug("MasterLogic Qry : SELECT COUNT(1) FROM " + sQry);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(1) FROM " + sQry, con);
            ret = Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[5]: " + Ex.Message.ToString();
            logger.Debug("MasterLogic Error: " + DBErrBuf);
            ret = -1;
        }
        finally
        {
            con.Close();
        }
        return ret;
    }
    
    public DataTable GetDataTable(string sQry)
    {
        DataTable dt = new DataTable();
        
        try
        {
            con.Open();
            logger.Debug("MasterLogic Qry : " + sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[7]: " + Ex.Message.ToString();
            logger.Debug("MasterLogic Error: " + DBErrBuf);
            dt = null;
        }
        finally
        {
            con.Close();
        }
        return dt;
    }

    public int FetchSingleColumn(string sQry, ref string buf)
    {
        try
        {
            buf = "";
            con.Open();
            logger.Debug("MasterLogic Qry : " + sQry);
            MySqlCommand cmd = new MySqlCommand(sQry, con);
            buf = cmd.ExecuteScalar().ToString();
            if (buf.Length == 0)
                return 0;

            return 1;
        }
        catch (Exception Ex)
        {
            DBErrBuf = "DC[3]: " + Ex.Message.ToString();
            logger.Debug("MasterLogic Error: " + DBErrBuf);
            return -1;
        }
        finally
        {
            con.Close();
        }
    }
    public DataSet GetMysqlDataSets(string Qry)
    {
        try
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            con.Open();
            cmd.CommandText = Qry;
            logger.Debug(Qry.ToString());
            MySqlDataAdapter Da = new MySqlDataAdapter(cmd);
            DataSet DsTerminalUserMasterDet = new DataSet();
            Da.Fill(DsTerminalUserMasterDet);
            return DsTerminalUserMasterDet;
        }
        catch (Exception ex)
        {
            logger.Debug("GetMysqlDataSets ERROR : " + ex.Message.ToString());
            return null;
        }
        finally
        {
            con.Close();
        }
    }
    public int ExecScalar(string sQry, ref string buf)
    {
        try
        {
            buf = "";
            con.Open();
            MySqlCommand cmd = new MySqlCommand(sQry, con);
            buf = cmd.ExecuteScalar().ToString();
            if (buf.Length == 0)
                return 0;

            return 1;
        }
        catch (Exception ex)
        {
            logger.Debug("getTableCount  ERROR : " + ex.Message.ToString());
            return -1;
        }
        finally
        {
            con.Close();
        }
    }
    public int execCmd(string strQrySelect, string strQryInsert, string strQryUpdateDelete, int Type)
    {
        try
        {
            int intSelect = -1;
            int affRows = 0;
            con.Open();
            if (Type == 0) //Insert if Type is 0
            {
                if (strQrySelect != null && strQrySelect.Trim() != "") // If select command is not null
                {
                    logger.Debug(strQrySelect.ToString());
                    MySqlCommand cmdSel = new MySqlCommand(strQrySelect, con);
                    if (cmdSel.ExecuteScalar() != null) // Checking not null
                        intSelect = Convert.ToInt32(cmdSel.ExecuteScalar());  // If null, returns the record status value 
                }
                if (intSelect == -1) // If record status is null then insert the record
                {
                    logger.Debug(strQryInsert.ToString());
                    MySqlCommand cmdInsert = new MySqlCommand(strQryInsert, con);
                    affRows = cmdInsert.ExecuteNonQuery();
                }
                else if (intSelect == 0) // If record status is 0 then update the record
                {
                    logger.Debug(strQryUpdateDelete.ToString());
                    MySqlCommand cmdUpdate = new MySqlCommand(strQryUpdateDelete, con);
                    affRows = cmdUpdate.ExecuteNonQuery();
                }
            }
            else if (Type == 1)//Update if Type is 1
            {
                int intUpdate = 0;
                if (strQrySelect != null && strQrySelect.Trim() != "") // If select command is not null
                {
                    logger.Debug(strQrySelect.ToString());
                    MySqlCommand cmdSelect = new MySqlCommand(strQrySelect, con);
                    intUpdate = Convert.ToInt32(cmdSelect.ExecuteScalar());
                }
                if (intUpdate == 0)
                {
                    logger.Debug(strQryUpdateDelete.ToString());
                    MySqlCommand cmdUpdate = new MySqlCommand(strQryUpdateDelete, con);
                    affRows = cmdUpdate.ExecuteNonQuery();
                }
            }
            else if (Type == 2)//Delete if Type is 2
            {
                logger.Debug(strQryUpdateDelete.ToString());
                MySqlCommand cmdUpdate = new MySqlCommand(strQryUpdateDelete, con);
                affRows = cmdUpdate.ExecuteNonQuery();
            }
            else
            {
            }
            return affRows;
        }
        catch (Exception ex)
        {
            logger.Debug("execCmd  ERROR : " + ex.Message.ToString());
            return -1;
        }
        finally
        {
            con.Close();
        }
    }
    public int ExecQry(string Qry)
    {
        int ret = 0;

        try
        {
            con.Open();
            logger.Debug(Qry.ToString());
            MySqlCommand cmd = new MySqlCommand(Qry, con);
            ret = cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            DBErrBuf = "DC[6]: " + ex.Message.ToString();
            logger.Debug("ExecQry  ERROR : " + ex.Message.ToString());
            ret = -1;
            
        }
        finally
        {
            con.Close();
        }
        return ret;
    }
    public int ExecMultipleQry(int Cnt, string[] sQry)
    {
        int AffRows = 0, i = 0;
        con.Open();
        MySqlTransaction objSqlT = con.BeginTransaction();
        try
        {
            MySqlCommand cmd;
            for (i = 0; i < Cnt; i++)
            {
                if (sQry[i] == string.Empty || sQry[i] == "")
                    continue;
                logger.Debug(sQry[i].ToString());
                cmd = new MySqlCommand(sQry[i], con);
                AffRows += cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            objSqlT.Commit();
        }

        catch (Exception ex)
        {
            objSqlT.Rollback();
            AffRows = -1; 
            DBErrBuf = "DC[7]: " + ex.Message.ToString();
            logger.Debug("ExecMultipleQry  ERROR : " + ex.Message.ToString());
        }

        finally
        {
            con.Close();
        }
        return AffRows;
    }
    public string FmtDBString(string SrcStr)
    {
        string DstBuf = "";

        DstBuf = SrcStr.Replace("'", "''");
        return DstBuf;
    }
    public int MaxId(string StrQry)
    {
        int Maxid = 0;
        try
        {
            con.Open();
            logger.Debug(StrQry.ToString());
            MySqlCommand cmd = new MySqlCommand(StrQry, con);
            Maxid = Convert.ToInt16(cmd.ExecuteScalar());
            return Maxid;
        }
        catch (Exception ex)
        {
            logger.Debug("MaxId  ERROR : " + ex.Message.ToString());
            return -1;
        }
        finally
        {
            con.Close();
        }
    }
    public List<T> ConvertTo<T>(DataTable datatable) where T : new()
    {
        List<T> Temp = new List<T>();
        try
        {
            List<string> columnsNames = new List<string>();
            foreach (DataColumn DataColumn in datatable.Columns)
                columnsNames.Add(DataColumn.ColumnName);
            Temp = datatable.AsEnumerable().ToList().ConvertAll<T>(row => getObject<T>(row, columnsNames));
            return Temp;
        }
        catch
        {
            return Temp;
        }

    }

    public T getObject<T>(DataRow row, List<string> columnsName) where T : new()
    {
        T obj = new T();
        try
        {
            string columnname = "";
            string value = "";
            PropertyInfo[] Properties;
            Properties = typeof(T).GetProperties();
            foreach (PropertyInfo objProperty in Properties)
            {
                columnname = columnsName.Find(name => name.ToLower() == objProperty.Name.ToLower());
                if (!string.IsNullOrEmpty(columnname))
                {
                    value = row[columnname].ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (Nullable.GetUnderlyingType(objProperty.PropertyType) != null)
                        {
                            value = row[columnname].ToString().Replace("$", "").Replace(",", "");
                            objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(objProperty.PropertyType).ToString())), null);
                        }
                        else
                        {
                            value = row[columnname].ToString().Replace("%", "");
                            objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(objProperty.PropertyType.ToString())), null);
                        }
                    }
                }
            }
            return obj;
        }
        catch
        {
            return obj;
        }
    }

}
