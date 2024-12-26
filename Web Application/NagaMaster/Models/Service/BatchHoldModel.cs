using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Service
{
    public class BatchHoldModel : LayoutModel
    {
        public List<SelectListItem> HoldTypeDetails { get; set; }
        public int HoldType { get; set; }
        public string PlantId { get; set; }
        public string BatchNo { get; set; }
        public string MaterialDesc { get; set; }
        public string MaterialCode { get; set; }
        public List<SelectListItem> PlantDetails { get; set; }

    }
}