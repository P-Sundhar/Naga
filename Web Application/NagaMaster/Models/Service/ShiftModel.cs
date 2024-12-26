using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Service
{
    public class ShiftModel : LayoutModel
    {

        public List<SelectListItem> UnitDetails { get; set; }
        public List<SelectListItem> SystemDetails { get; set; }
        public string UnitId { get; set; }
        public string SystemId { get; set; }
        public string ShiftDate { get; set;  }
        public int ShiftId { get; set; }
        public int Type { get; set; }
    }
    
}