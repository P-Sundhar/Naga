using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Service
{
    public class PickListModel : LayoutModel
    {
        public List<SelectListItem> SendLctDetails { get; set; }
        public List<SelectListItem> LineDetails { get; set; }
        public List<SelectListItem> SipmentDetails { get; set; }
        public string SendLocation { get; set; }
        public string BoxQty { get; set; }
        public string DlNumber { get; set; }
        public string Sipment { get; set; } 
        public string FromPlant { get; set; } 
    }
}