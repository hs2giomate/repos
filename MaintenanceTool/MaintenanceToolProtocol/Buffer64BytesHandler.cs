using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MaintenanceToolProtocol
{
    
   // [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public  class Full64BufferMessage
    {
        public SingleTaskCommand header;
        public  Byte[] content;
    }
    public  class Buffer64BytesHandler
    {
        private static Buffer64BytesHandler handler;
        private Full64BufferMessage message64;
        private CommandHeader commandHeader;
        private  int remainingSize;
        
        public Buffer64BytesHandler()
        {
            handler = this;
            commandHeader = new CommandHeader();
            message64 = new Full64BufferMessage();
            message64.header = commandHeader.order;
            remainingSize=64- Marshal.SizeOf(message64.header);
            message64.content = new Byte[remainingSize];

        }

        public static Full64BufferMessage Message64
        {
            get
            {
                if (handler == null)
                {
                    handler = new Buffer64BytesHandler();
                }
                return handler.message64;
            }
        }


    }
}
