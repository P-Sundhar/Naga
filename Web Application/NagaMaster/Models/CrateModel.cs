using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class CrateModel : LayoutModel
    {
        public int Id { get; set; }
        public string CrateCode { get; set; }
        public string CurrentStatus { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }


    }
}