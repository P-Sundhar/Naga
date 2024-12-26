using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class PlantsModel : LayoutModel
    {
        public int Id { get; set; }

        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public string CompanyID { get; set; }
        public string ShortName { get; set; }
        public string PlantType { get; set; }
        public string PlantAlias { get; set; }
        public int Dashboard { get; set; }
        public int Status { get; set; }
        public int ServerSendFlag { get; set; }
        public List<Company> CompanyDetail { get; set; }
    }
    public class Company
    {
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
    }
}