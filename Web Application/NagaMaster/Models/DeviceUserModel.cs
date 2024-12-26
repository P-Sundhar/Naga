using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models
{
    public class DeviceUserModel : LayoutModel
    {
        public string PlantId { get; set; }
        public string EmpId { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public string UserPWD { get; set; }
        public string UsrMstrId { get; set; }
        public string Status { get; set; }
        public string PlantName { get; set; }
        public string EmpDesc { get; set; }
        public List<SelectListItem> PlantDetail { get; set; }
        public List<SelectListItem> EmployeeDetail { get; set; }
    }
}