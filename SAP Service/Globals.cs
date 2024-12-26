using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;


public class Globals
{
    public const string APPLN_VERSION = "1.11";
    public const string APPLN_DATE = "23.12.2024";
    public static Boolean OTA_TEST = false; // Live = False;  Test = True;

    // Web API Method
    public static string MTHD_DELIVERYORDER = "/deliverydetail/barcode?sap-client=900";
    public static string MTHD_DELIVER = "/scanbatch/barcode?sap-client=900";
    public static string MTHD_RECEIVING = "/recscanbatch/barcode?sap-client=900";
    public static string MTHD_PRODUCTION = "/proddetail/barcode?sap-client=900";
    public static string MTHD_SALERETURN= "/zbc_sales_ret/barcode?sap-client=900";

}

