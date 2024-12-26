using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class StockSmartModel
    {
        public Int64 SNo { get; set; }
        public string Plant { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public string BatchNo { get; set; }
        public string Crate { get; set; }
        public string Secondary { get; set; }
        public string ManufacturingDate { get; set; }
        public string ExpiryDate { get; set; }
        public Int64 RemainingDays { get; set; }
        public Int64 AgingDays { get; set; }
        public Int64 StockQtyInBox { get; set; }
        public Int64 StockQtyInEach { get; set; }
        public decimal Restricted { get; set; }
        public decimal UnRestricted { get; set; }

    }
}