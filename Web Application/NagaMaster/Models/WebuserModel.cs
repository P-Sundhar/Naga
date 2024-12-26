using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Models.User
{
    public class WebuserModel : LayoutModel
    {
        public int id { get; set; }
        public int MenuType { get; set; }
        public int MenuId { get; set; }
        public int RoleId { get; set; }
        public string MenuDesc { get; set; }
        public int OptEdit { get; set; }
        public int OptView { get; set; }
        public int Status { get; set; }
        public decimal EmpId { get; set; }
        public string EmpDesc { get; set; }
        public string RoleName { get; set; }
        public string DisplayName { get; set; }
        public int Edit { get; set; }
        public int View { get; set; }
        public List<SelectListItem> UserRoleDetail { get; set; }
    }
    public class UserRoleDetails
    {
        public int id { get; set; }
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public string MenuDesc { get; set; }
        public int OptEdit { get; set; }
        public int OptView{ get; set; }
        public bool View { get; set; }
        public bool Edit { get; set; }

    }
}