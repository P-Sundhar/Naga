using System;
using System.Collections.Generic;
using System.Text;

namespace SalesReturn.Models
{
    public class ResponseModel
    {
        public int Status { get; set; }
        public string Message { get; set; } 
    }
    public class BarcodeValidate
    {
        public string ScannedBy { get; set; }
        public string PlantId { get; set; }
        public string RackBarcode { get; set; }
        public string CrateBarcode { get; set; }
        public string SecondaryBarcode { get; set; }
        public int AcceptorHold { get; set; }
        public string OutletCode { get; set; }
        public string OutletName { get; set; }
        public string TruckNo { get; set; }
        public string ReceivingPlant { get; set; }
        public string StorageLoc { get; set; }
    }
    public class PlantResponse : ResponseModel
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
    public class SalesReturnResponse : ResponseModel 
    {
        public List<SalesReturn> data { get; set; }
    }
    public class SalesReturn 
    {
        public string PlantId { get; set; }

        public string OutletCode {get; set; }

        public string OutletName { get; set; }
        public string RackBarcode { get; set; }
        public string CrateBarcode { get; set; }
        public string SecondaryBarcode { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class LoginModel
    {
        public string Imei { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }
    public class OutletResponse : ResponseModel
    {
        public List<OutletModel> data { get; set; }
    }
    public class OutletModel
    {
        public string OutletCode { get; set; }
        public string OutletName { get; set; }
        public int Status { get; set; }
    }
}
