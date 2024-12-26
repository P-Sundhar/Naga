using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models
{
    public class UnitModel : LayoutModel
    {
        public int Id { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public int WHouseType { get; set; }
        public string Systems { get; set; }
        public string DocNo { get; set; }
        public string UnitColor { get; set; }
        public int Status { get; set; }
        public int ServerSendFlag { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public List<SelectListItem> System { get; set; }
        public List<SelectListItem> PlantDetails { get; set; }
        //public List<PlantMasterModel> PlantDetails { get; set; }
    }

    public class Systems {
        public int SystemId { get; set; }
        public string SystemName { get; set; }
        public bool Selected { get; set; }
    }

    public class PlantDetails
    {
        public string PlantId { get; set; }
        public string PlantName { get; set; }
    }
}