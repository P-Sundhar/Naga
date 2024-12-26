using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class RackModel : LayoutModel
    {
        public int Id { get; set; }
        public string RackNumber { get; set; }
        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public string PlantType { get; set; }
        public string StorageLocation { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public int Status { get; set; }
        public List<Plant> Plantlist { get; set; }
    }
    public class Plant : LayoutModel
    {
        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public string PlantType { get; set; }
    }
}