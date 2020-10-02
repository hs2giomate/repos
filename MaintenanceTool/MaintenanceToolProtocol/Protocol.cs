using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceToolProtocol
{
    public class Protocol
    {
        private static Protocol handler;
        public Byte[] buffer;
        public Protocol()
        {
            handler = this;
           buffer= new Byte[Constants.BufferSize];
        }

        public static Protocol Message
        {
            get
            {
                if (handler==null)
                {
                    handler = new Protocol();
                }
                return handler;
            }
        }

        public Byte[] BuildAliveMessage()
        {
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            // datagram.order.task = 'I';
            order.command = Commands.KeepConnected;
            datagram.order = order;
            buffer.Initialize();
            Buffer.BlockCopy(datagram.GetOrderArrayBytes(), 0, buffer, 0, buffer.Length);
            return buffer;

        }
    }
}
