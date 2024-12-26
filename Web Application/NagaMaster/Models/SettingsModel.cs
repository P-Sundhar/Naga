using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class SettingsModel : LayoutModel
    {
        public int Id { get; set; }
        public string ComPort { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public int StopBits { get; set; }
        public string System { get; set; }
        public string UnitId { get; set; }
        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public int Status { get; set; }

    }
}