using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesReturnAPI
{
    public class ResponseHelper
    {
        public int status { get; set; }
        public string message { get; set; }
    }
    public class OutletResponse : ResponseHelper
    {
        public List<OutletModel> data { get; set; }
    }
    public class PlantRespose : ResponseHelper
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
    public class OutletModel
    {
        public string OutletCode { get; set; }
        public string OutletName { get; set; }
        public int Status { get; set; }
    }
    public class SalesReturnResponse : ResponseHelper
    {
        public List<SalesReturn> data { get; set; }
    }
    public class SalesReturn
    {
        public string PlantId { get; set; }
        public string OutletCode { get; set; }
        public string OutletName { get; set; }
        public string RackBarcode { get; set; }
        public string TruckNo { get; set; }
        public string SecondaryBarcode { get; set; }
        public string CrateBarcode { get; set; }
        public DateTime CreatedOn { get; set; }
        public int AcceptorHold { get; set; }

    }
    public class LoginModel
    {
        public string Imei { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }

}