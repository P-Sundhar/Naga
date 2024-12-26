using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Audit
{
    public class AuditModel : LayoutModel
    {
        public List<SelectListItem> PlantDetails { get; set; }
        public List<SelectListItem> DwnldFmtDetail { get; set; }

        public int DwnldFmt { get; set; }
        public string PlantId { get; set; }
    }
    public class AuditModel_Grid
    {
        public string Plant { get; set; }
        public string CrateBarcode { get; set; }
        public string Barcode { get; set; }
        public string BatchNo { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public string sysPlant { get; set; }
        public string sysCrate { get; set; }
        public string sysBarcode { get; set; }
        public string sysBatch { get; set; }
        public string sysMCode { get; set; }
        public string sysMDesc { get; set; }
        public int Flags { get; set; }
    }
}