using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Reports
{
    public class RjtReportModel : LayoutModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int DwnldFmt { get; set; }
        public List<SelectListItem> DwnldFmtDetail { get; set; }

        public string BatchNo { get; set; }
        public string ProductionDate { get; set; }

    }
}