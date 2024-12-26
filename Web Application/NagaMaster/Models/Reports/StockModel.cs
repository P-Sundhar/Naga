using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Reports
{
    public class StockModel : LayoutModel
    {
        public List<SelectListItem> PlantDetails { get; set; }
        public List<SelectListItem> UnitDetails { get; set; }
        public List<SelectListItem> SystemDetails { get; set; }
        public List<SelectListItem> DwnldFmtDetail { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int DwnldFmt { get; set; }
        public string PlantId { get; set; }
        public string UnitId { get; set; }
        public string SystemId { get; set; }
        public int IsDwnld { get; set; }
    }
}