using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class LayoutModel
    {
        public List<Menus> menu { get; set; }
    }

    public class Menus 
    {
        public int MenuType { get; set; }
        public int MenuId { get; set; }

        public int IsVisible { get; set; }
    }
}