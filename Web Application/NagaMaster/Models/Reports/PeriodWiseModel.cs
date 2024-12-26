using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Reports
{
    public class PeriodWiseModel : LayoutModel
    {

        public List<SelectListItem> PlantDetails { get; set; }
        public List<SelectListItem> MaterialDetails { get; set; }
        public List<SelectListItem> UnitDetails { get; set; }
        public List<SelectListItem> StockStatusDetails { get; set; }
        public List<SelectListItem> DwnldFmtDetail { get; set; }
        public List<SelectListItem> SystemDetails { get; set; }
        
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int DwnldFmt { get; set; }
        public string PlantId { get; set; }
        public string SystemId { get; set; }
        public string UnitId { get; set; }
        public string MaterialCode { get; set; }
        public string StockStatus { get; set; }
        public int IsDwnld { get; set; }
        public string BatchNo { get; set; }
        public string MaterialCategory { get; set; }

        public int BatchType { get; set; }
        
    }
}