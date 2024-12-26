using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.Service
{
    public class AssignmentModel : LayoutModel
    {
        public List<SelectListItem> UnitDetails { get; set; }
        public List<SelectListItem> SystemDetails { get; set; }
        public List<SelectListItem> ItemDetails { get; set; }
        public string UnitId { get; set; }
        public int PDNId { get; set; }
        public string SystemId { get; set; }
        public string Material { get; set; }
        public string MaterialDesc { get; set; }
        public string BatchNo { get; set; }
        public int ShiftId { get; set; }
        public int Target { get; set; }
        public string Primary { get; set; }
    }

    public class AssignMentGrid 
    {
        public string Date { get; set; }

        public string System { get; set; }

        public string Material { get; set; }

        public string BatchNo { get; set; }

        public string MonthYear { get; set; }
        public string TargetQty { get; set; }
        public string PrimaryQty { get; set; }
        public string UnitName { get; set; }
    }
}