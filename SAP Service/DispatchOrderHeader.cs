using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NagaSapService
{
    public class DispatchOrderHeader
    {
        public string DELIVERY_NO { get; set; }
        public string TYPES_OF_TRANSACTION { get; set; }
        public string SHIPMENT_OR_PO { get; set; }
        public string DELIVERY_DATE { get; set; }
        public string SENDING_PLANT { get; set; }
        public string SENDING_LOCATION { get; set; }
        public string RECEIVING_PLANT { get; set; }
        public string RECEIVING_LOCATION { get; set; }
        public string TRUCK_NUMBER { get; set; }
        public string OUTLET_NAME { get; set; }
        public string INDENT_NUMBER { get; set; }
        public List<DispatchOrderLines> LINE_ITEM { get; set; }

    }

    public class DispatchOrderLines 
    {
        public string DELIVERY_NO { get; set; }
        public string LINE { get; set; }
        public string MATERIAL_CODE { get; set; }
        public string DELIVERY_QTY_IN_BOX { get; set; }
    }
}
