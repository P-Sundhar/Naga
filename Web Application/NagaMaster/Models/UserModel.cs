using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models
{
    public class UserModel : LayoutModel
    {
        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string Systems { get; set; }
        public string UserId { get; set; }
        public string UserPWD { get; set; }
        public decimal EmpId { get; set; }
        public string EmpDesc { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public List<PlantModels> PlantDetail { get; set; }
        public List<UnitModel>UnitDetail { get; set; }
        public List<EmployeeModel>EmployeeDetail { get; set; }
        public List<SelectListItem> SystemDetail { get; set; }

        public class PlantModels
        {
            public string PlantId { get; set; }
            public string PlantName { get; set; }
        }
        public class UnitModel
        {
            public string UnitId { get; set; }
            public string UnitName { get; set; }
        }
        public class EmployeeModel
        {
            public string EmpId { get; set; }
            public string EmpDesc { get; set; }
        }
        public class System
        {
            public int SystemId { get; set; }
            public string SystemName { get; set; }
            public bool Selected { get; set; }
        }
    }
}