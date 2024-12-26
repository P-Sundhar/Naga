using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models.StockImport
{
    public class NoBarcodeModel : LayoutModel
    {
        public string PlantId { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public string Barcode { get; set; }
        public string CrateBarcode { get; set; }
        public string BatchNo { get; set; }
        public string PDNDate { get; set; }
        public decimal PDNCnt { get; set; }
        public decimal PickedQty { get; set; }
        public decimal BalanceQty { get; set; }
        public int Status { get; set; }
    }
}