using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Service
{
    public class RejectModel : LayoutModel
    {
        public List<SelectListItem> BatchDetails { get; set; }
        public List<SelectListItem> MaterialDetails { get; set; }
        public List<SelectListItem> RejectTypeDetails { get; set; }
        public List<SelectListItem> MaterialList { get; set; }
        public List<SelectListItem> BarcodeListDetails { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string RejectType { get; set; }
        public string BatchNo { get; set; }
        public string Barcode { get; set; }
        public string tblReject { get; set; }
        public string values     { get; set; }
        public string Material { get; set; }
    }

    public class BarcodeListDetails
    {
        public string Barcode { get; set; }
    }
}