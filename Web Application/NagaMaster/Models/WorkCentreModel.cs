using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class WorkCentreModel
    {
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public string MachineNumber { get; set; }
        public string MachineName { get; set; }
        public int Status { get; set; }

    }
}