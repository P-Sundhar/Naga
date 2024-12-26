using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Globals
{
    // Download Format
    public static int DWNLDFMT_PDF = 0;
    public static int DWNLDFMT_EXCEL = 1;

    // Master / Transaction Status
    public static string STR_ACTIVE = "Active";
    public static string STR_DEACTIVE = "In Active";

    public static string gsAppTile = "NAGA";

    // Report Path
    public static string RPTPATH = "Reports";

    // Rejection Type
    public static int RJTYPE_EXPIRED_REJECT = 0;
    public static int RJTYPE_QUALITY_REJECT = 1;

    // Stock Status
    public static int RESTRICTED = 1;
    public static int UNRESTRICTED = 0;

    public static string EXPIRED_REJECT_DESC = "Expired Reject";
    public static string QUALITY_REJECT_DESC = "Quality Reject";

    //Hold Types
    public static string QUALITY_HOLD_DESC = "Quality Hold";
    public static string DAMAGE_HOLD_DESC = "Damage Hold";
    public static string FG_DISCARD_DESC = "FG Discard";


    public static int QUALITY_HOLD = 1;
    public static int DAMAGE_HOLD = 2;
    public static int FG_DISCARD = 3;
    public static int HOLD_RELEASE = 0;

    //Audit Types                                   IN Table Ajusted Column value Updated
    public static int AUDIT_NO_ISSUE = 0;           //   0
    public static int AUDIT_CRATE_MISMATCH = 1;     //   2
    public static int AUDIT_PHY_MISSING = 2;        //   1
    public static int AUDIT_SYS_MISSING = 3;        //  -1


    //Missing Crates
    public static int MOB_TOFIND = 0;
    public static int MOB_RECEIVED = 1;
    public static int MOB_DAMAGE = 2;
    public static int WEB_MISSING = 3;
    public static int WEB_REVOKE = 4;
    public static int WEB_RECEIVED = 4;


    //Roles MENU_ID
    public static int DashBoard = 1;
    public static int COM_Settings = 2;
    public static int Employee_Master = 3;
    public static int Item_Master = 4;
    public static int Machine_Master = 5;
    public static int Unit_Master = 6;
    public static int Master_Upload = 7;
    public static int System_User_Master = 8;
    public static int Responsibility = 9;
    public static int CrateMaster = 10;
    public static int PlantMaster = 11;
    public static int RackMaster = 12;
    public static int UserRole = 13;
    public static int DeviceUserMaster = 14;

    public static int Shift_Details = 101;
    public static int AssignMent = 102;
    public static int Job_Completion = 103;
    public static int STOPicklist = 104;
    public static int SALESPicklist = 105;
    public static int STORejection = 106;
    public static int CFARejection = 107;
    public static int ShipmentPoApproval = 108;
    public static int CrateMissing = 109;

    public static int Plant_Wise_Report = 201;
    public static int PutAwayReport = 202;
    public static int STORejectionReport = 203;
    public static int CFA_Rejection_Report = 204;
    public static int STOCK_SMART_RPT = 205;
    public static int CFA_Stock_Report = 206;
    public static int Production_Dispatch_Report = 207;
    public static int CFA_Dispatch_Report = 208;

    public static int Import_WithoutBarcode = 301;
    public static int Import_WithBarcode = 302;
    public static int Stock_Audit = 303;

}
