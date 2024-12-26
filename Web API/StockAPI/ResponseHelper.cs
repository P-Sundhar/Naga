using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockAPI
{
    public class ResponseHelper
    {
        public int status { get; set; }
        public string message { get; set; }
    }

    public class PlantRespose :ResponseHelper
    {
        public List<PlantMaster> data { get; set; }
    }
    public class PlantMaster
    {
        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public string ShortName { get; set; }
        public string PlantType { get; set; }
        public string PlantAlias { get; set; }
        public int PlantTypeFlag { get; set; }
        public int Status { get; set; }
    }

    public class BarcodeValidate
    {
        public string ScannedBy { get; set; }
        public string PlantId { get; set; }
        public string RackBarcode { get; set; }
        public string CrateBarcode { get; set; }
        public string SecondaryBarcode { get; set; }
    }
    public class StockAuditResponse : ResponseHelper
    {
        public List<StockAudit> data { get; set; }
    }
    public class StockAudit
    {
        public string Plant { get; set; }
        public string RackNo { get; set; }
        public string CrateBarcode { get; set; }
        public string Barcode { get; set; }
        public string BatchNo { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public DateTime ScannedOn { get; set; }

    }

    public class LoginModel
    {
        public string Imei { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }

    public class PhysicalAdjust 
    {
        public string Plant { get; set; }
        public string RackNo { get; set; }
        public string CrateBarcode { get; set; }
        public string Barcode { get; set; }
        public string BatchNo { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public string AuditOn { get; set; }
        public string StockAdded { get; set; }
        public string StockReduced { get; set; }
    }

    public class PhysicalAdjustResponse : ResponseHelper
    {
        public List<PhysicalAdjust> data { get; set; }
    }
}