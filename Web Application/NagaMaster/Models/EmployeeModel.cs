using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models
{
    public class EmployeeModel : LayoutModel
    {
        public decimal EmpId { get; set; }
        public string EmpDesc { get; set; }
        public int EmpType { get; set; }
        public int Status { get; set; }
        public int id { get; set; }
        public string RoleName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        //public List<RolenameModel> PlantDetail { get; set; }
        public List<SelectListItem> RoleDetail { get; set; }
    }
    public class RoleDetail
    {
        public int id { get; set; }
        public string RoleName { get; set; }
    }
}