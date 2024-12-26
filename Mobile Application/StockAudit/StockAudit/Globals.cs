using System;
using System.Collections.Generic;
using System.Text;

namespace StockAudit
{
    public class Globals
    {
        // *******************  APPLN VERSION DETAILS ********************************
        static public string APP_VERSION = "01.00";
        static public string APP_RLS_DATE = "26-09-2023";
        // ***************************************************************************

        static public string AppPathDirectory = "";
        // *****************   CHK HERE BEFORE RELEASE  ************************************
        static public bool CHK_LICENCE = false;               // true - live, false - test
        static public bool OTA_TESTING = false;              // false - live, true - ota FTP
        static public bool FTP_TESTING = true;               // true - live, false - no need ftp connect
        static public bool FTP_DELETE_FILE = true;           // true - live,& can delete, false - can't delete [testing]
        static public bool CHK_AUTO_DATETIME = false;         // true - live, false - testing dont chk auto date time
        static public bool DATA_SYNC_ENABLE = true;          // true - live, false - testing
        static public bool HARDCODE_LOGIN = true;           // false - live, true - testing
        static public bool ROOT_ACCESS = false;              // always false
        static public bool HOME_RESTART = ROOT_ACCESS;       // true - Restart on Clicking Home Button, false - Exists App Alone
        static public bool HIT_CLIENT_TRACK = true;          // true - live, false - testing
        static public bool HIDE_TOP_NAVIGATIONBAR = false;   // true - disallow touch in top rows
        static public bool TAB_FULLSCREEEN = false;          // true - block bottom bar usage in tab, false - default


        public static string IMEI = "";
        public static string PageName = "";

        public static int HandWritingInputFiled = 0;
        public static string HandWritingValue = "";
        public static int ErrorManditry = 0;

        static public string APPLN_PATH = "/Naga/";
        static public string DWNLD_FILE = APPLN_PATH + "Download.txt";
        static public string UPLOAD_FILE = APPLN_PATH + "Upload.txt";
        static public string APPDOWNLOAD_FILE = APPLN_PATH + "/AppDwnld.txt";
        static public string DWNLD_APPLN_NAME = APPLN_PATH + "HatsunGrn.apk";

        static public string DB_UPLD_PWD = "001101";

        // ** FTP Related
        static public int FTP_ASCII_FORMAT = 1;
        static public int FTP_BINARY_FORMAT = 2;


        // ###################    HOST      ######################
        public static int CT_APPLN_TYPE = 0;


        // ###################    SERVER      ######################
        public static string ServerUrl = "";
        public static string Plant = "";
        public static string GetPlant = "Home/GetPlant";
        public static string GetCrateValidate = "Home/ValidateCrate";
        public static string GetBarcodeValidate = "Home/ValidateBarcode";
        public static string GetStockDetails = "Home/GetStockDetails";
        public static string SaveStockAudit = "Home/SaveStockAudit";
        public static string Login = "Home/LoginValidate";
        public static string CLNT_DB_UPLD_NAME = "Clnt";
        public static string CLNT_APP_DWNLD_FILE = "AppDwnld.txt";
        public static bool MNUAL_KEYBOARD = false;
        public static bool CrateEnabled = false;
        public static bool SecondaryEnabled = false;
        public static string UserEmail = "";

        // App Globals *****
        static public int gSyncFlag;


        //	**************	Web service Msg Types
        public static int MTI_DOWNLOAD = 43;
        public static int MTI_UPLOAD = 44;

        // Dialog Input Details
        static public int DLGIPTYPE_NUMERIC = 1;
        static public int DLGIPTYPE_ALPHA = 2;
        static public int DLGIPTYPE_ALPHANUMERIC = 3;

        static public string DLGIP_TITLE = "DlgIpTitle";
        static public string DLGIP_IPMODE = "DlgIpMode";
        static public string DLGIP_ISPWD = "DlgIpIsPwd";
        static public string DLGIP_MAXLEN = "DlgIpMaxLen";
        static public string DLGIP_CMPDATA = "DlgIpCmpData";
        static public string DLGIP_RSLT_INTENT_RSP = "DlgIpRsp";
        static public int DLGIP_RSLT_INTENT_CALL = 1000;


        //  *********** Common Length
        public static int LEN_COMM_PACKET_LEN = 3800;
        public static int LEN_DATETIME = 14;
        public static int LEN_FMT_DATETIME = 19;
        public static int LEN_DATE = 8;
        public static int LEN_FMT_DATE = 10;
        public static int LEN_TIME = 6;
        public static int LEN_FMT_TIME = 8;
        public static int LEN_AMT = 12;
        public static int LEN_FMT_AMT = 13;    // 12 + 1 dp
        public static int LEN_PERCENT = 5;
        public static int LEN_FMT_PERCENT = 6;    // 5 + 1 dp

        public static int LEN_TABID = 32;
        public static int LEN_PROC_CODE = 2;
        public static int LEN_PACK_CNT = 6;
        public static int LEN_RESPONSE_CODE = 2;
        public static int LEN_USER_ID = 10;
        public static int LEN_USER_PWD = 10;
        public static int LEN_RSPERROR_MSG = 80;


        // ***************** Application Related Global Variables

        static public int IDX_DWNLD_UPLD = 100;


        // Util Menu Options
        static public int UMNU_SYNC_FULL = 1;
        static public int UMNU_SYNC_PARTIAL = 2;
        static public int UMNU_SYNC_TRAN = 3;
        static public int UMNU_DB_UPLOAD = 7;
        static public int UMNU_LOGOUT = 8;


        // Sync Process
        static public int SYNC_MANUAL = 1;
        static public int SYNC_BACKGROUND = 2;

        // Shared Preference
        static public string ECAP_LOGGED_USERID = "LgnUserId";
        static public string ECAP_LOGGED_PASSWORD = "LgnPwd";
        static public string ECAP_LOGGED_EMP_ID = "LgnEmpId";
        static public string ECAP_LOGGED_EMP_NAME = "LgnEmpName";
    }
}
