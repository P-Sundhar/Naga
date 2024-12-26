using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Naga
{
    class Globals
    {
        public const string APPLN_VERSION = "1.13";
        public const string APPLN_DATE = "09.02.2018";

        public const bool OTA_TESTING = true;       // false - live;  true - testing
        public const bool PRINT_ENABLE = true;      // true - live; false - testing
        public const bool CHK_LICENCE = false;       // true - live;  false - testing
        

        public static string Application_Author = "Origin Technology Associates";
        public static string PRINTER_NAME1 = ConfigurationManager.AppSettings["PRINTER_NAME1"].ToString();
        public static string PRINTER_NAME2 = ConfigurationManager.AppSettings["PRINTER_NAME2"].ToString();
        public static string SettingPath = ConfigurationManager.AppSettings["ConfigPath"].ToString();
        public static string SERVICE_URL = "http://103.231.100.112/ClientTrack_Host/Service.asmx";
        public static string SERVICE_METHOD = "ClientTrack";
        public static string APPLNID = "22";
        public static string CLIENTID = "1710041";
        public static string Device = "";

        public static int CottonBoxFlag;

        public static string STR_ACTIVE = "Active";
        public static string STR_DEACTIVE = "In Active";
        public static string STR_DDL_ALL = "---  All  ---";
        public static int DDL_INT_SELECT = 0;
        public static string DDL_STR_SELECT = "--SELECT--";

        
        public static string System;
        public static string Unit;
        public static string Plant;
        public static string IP;
        public static string Database;
        public static string UserHeader = "";
        public static string User = "";
        public static int EndShift;
        public static int LoginCheck = 1;
        public static int StartEndFlag = 0;
        public static int WeightFlag = 0;
    }
}
