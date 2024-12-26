using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naga
{
    class ComClass
    {
        MasterLogic objMas = new MasterLogic();
        SerialPort serial = new SerialPort();


        public SerialPort COM()
        {
            DataTable Dt = new DataTable();
            Dt = objMas.GetDataTable("Select ComPort,BaudRate,DataBits,StopBits From settings WHERE System='" + Globals.System + "' AND UnitId='" +
                Globals.Unit + "' and PlantId='" + Globals.Plant + "'");
            if (Dt == null)
            {
                serial = null;
                return serial;
            }
            if (Dt.Rows.Count == 0)
            {
                serial = null;
                return serial;
            }

            serial.PortName = Dt.Rows[0]["ComPort"].ToString(); //Com Port Name                
            serial.BaudRate = int.Parse(Dt.Rows[0]["BaudRate"].ToString());
            serial.Parity = Parity.None;
            serial.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Dt.Rows[0]["StopBits"].ToString());
            serial.DataBits = int.Parse(Dt.Rows[0]["DataBits"].ToString());
            return serial;
        }
    }
}
