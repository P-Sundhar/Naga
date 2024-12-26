using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.StockImport
{
    public class BarcodeWithDataModel : LayoutModel
    {
        public string GRNNO { get; set; }
        public string GRNDate { get; set; }
        public string FromShipmentNo { get; set; }
        public string FromDeliveryNo { get; set; }
        public string FromPlant { get; set; }
        public string ReceivingPlant { get; set; }
        public string PDNDate { get; set; }
        public string MaterialCode { get; set; }
        public string Barcode { get; set; }
        public string CrateBarcode { get; set; }
        public string NewCrateBarcode { get; set; }
        public string RackNo { get; set; }
        public string PutawayDate { get; set; }
        public string PutAwayFlag { get; set; }
        public decimal Weight { get; set; }
        public string BatchNo { get; set; }
        public string PlantId { get; set; }
        public List<SelectListItem> PlantDetail { get; set; }
    }
}