using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Service
{
    public class ApprovalModel : LayoutModel
    {
        public List<SelectListItem> PlantDetails { get; set; }
        public List<SelectListItem> SipmentDetails { get; set; }
        public string Shipment { get; set; }
        public string PlantCode { get; set; }
    }

    public class ApprovalDetails 
    {
        public string DeliveryNo { get; set; }  
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public string BatchNo { get; set; }
        public Int64 AvailableQty { get; set; }
        public Int64 ProposedQty { get; set; }
        public Int64 PickedQty { get; set; }  
        public Int64 DifferenceQty { get; set; }
        public string Outlet { get; set; }
    }
}