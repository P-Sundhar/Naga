using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;


// Page : A --> A []  C [ ]
public class AppDet
{

    const string PAGEDESC = "AppDet";

  

    public DataTable LoadStatus(MasterLogic objMas)
    {
        try
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("StatusDesc");
            dt.Columns.Add("StatusId");

            dt.Rows.Add(new object[] { Globals.STR_ACTIVE, "1" });
            dt.Rows.Add(new object[] { Globals.STR_DEACTIVE, "0" });
            return dt;
        }
        catch (Exception ex)
        {
            objMas.PrintLog(PAGEDESC, "AC[1] : " + ex.Message);
            return null; }
    }


    public DataTable LoadDwnldFormats(MasterLogic objMas)
    {
        try
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("DwnldDesc");
            dt.Columns.Add("DwnldId");

            dt.Rows.Add(new object[] { "PDF", Globals.DWNLDFMT_PDF });
            dt.Rows.Add(new object[] { "Excel", Globals.DWNLDFMT_EXCEL });
            return dt;
        }
        catch (Exception ex)
        {
            objMas.PrintLog(PAGEDESC, "AC[2] : " + ex.Message);
            return null;
        }
    }

    public DataTable LoadStockStatus(MasterLogic objMas)
    {
        try
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("TypeName");
            dt.Columns.Add("TypedId");

            dt.Rows.Add(new object[] { "Restricted", Globals.RESTRICTED });
            dt.Rows.Add(new object[] { "Unrestricted", Globals.UNRESTRICTED });
            return dt;
        }
        catch (Exception ex)
        {
            objMas.PrintLog(PAGEDESC, "AC[2] : " + ex.Message);
            return null;
        }
    }



}