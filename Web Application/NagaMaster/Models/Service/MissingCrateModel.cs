using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models.Service
{
    public class MissingCrateModel : LayoutModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int ProccessType { get; set; }
        public string Shipment { get; set; }
    }

    public class MissingCrate_Grid
    {
        public string CrateCode { get; set; }

    }

    public class MissingCrate_Save
    {
        public string CrateCode { get; set; }
        public string UpdateVal { get; set; }
    }
}