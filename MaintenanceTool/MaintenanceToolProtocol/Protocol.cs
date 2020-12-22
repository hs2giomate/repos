using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;

namespace MaintenanceToolProtocol
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SingleTaskMessage
    {
        public SingleTaskCommand header;
        public Byte description;
    }
   

    public class Protocol
    {
        private static Protocol handler;
        private int sizeStruct;
        private SingleTaskMessage singleTaskMessage;
        private Full64BufferMessage message64;
        public Byte[] buffer;
        public Protocol()
        {
            handler = this;
           buffer= new Byte[Constants.BufferSize];
            sizeStruct = Marshal.SizeOf(singleTaskMessage);
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
        public Byte[] CreateEnableHeatersMessage(Byte p)
        {
            var size = Marshal.SizeOf(singleTaskMessage);
            buffer = new Byte[size];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.EnableHeaters;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = p;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, size);
            return buffer;

        }
        public Byte[] CreateSetpointFansMessage(Byte p)
        {
            var size = Marshal.SizeOf(singleTaskMessage);
            buffer = new Byte[size];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.SetpointFans;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = p;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, size);
            return buffer;

        }
        public Byte[] CreateEnableFansMessage(Byte p)
        {
            var size = Marshal.SizeOf(singleTaskMessage);
            buffer = new Byte[size];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.EnableFans;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = p;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, size);
            return buffer;

        }
        public Byte[] CreateCommandFlapperValveMessage(Byte[] p)
        {
           // var size = Marshal.SizeOf(singleTaskMessage);
            buffer = new Byte[64];
            buffer.Initialize();
            //  Buffer64BytesHandler datagram = new CommandHeader();
            //    SingleTaskCommand order = datagram.order;

            //  order.task = Commands.CommandFlapperValve;

            message64 = Buffer64BytesHandler.Message64;
            message64.header.task= Commands.CommandFlapperValve;
            Buffer.BlockCopy(p, 0, message64.content, 0,58);
         //   datagram.order = order;
       //     SingleTaskMessage m;
      //      m.header = order;
        //    m.description = p;
           
            Buffer.BlockCopy(GetFullBufferCommandArrayBytes(message64), 0, buffer, 0, 64);
            return buffer;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Byte[] CreateSetpointtFlapperValveMessage(Byte[] p)
        {

            buffer = new Byte[64];
            buffer.Initialize();
            message64 = Buffer64BytesHandler.Message64;
            message64.header.task = Commands.SetPointFlapperValve;
            Buffer.BlockCopy(p, 0, message64.content, 0, 58);

            Buffer.BlockCopy(GetFullBufferCommandArrayBytes(message64), 0, buffer, 0, 64);
            return buffer;

        }
        public Byte[] CreateHeatersStatusRequestMessage()
        {           
            buffer = new Byte[sizeStruct];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.ReadStatusHeaters;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = 0;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, sizeStruct);
            return buffer;

        }
        public Byte[] CreateTemperatureRequestMessage()
        {
            buffer = new Byte[sizeStruct];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.ReadTemperatures;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = 0;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, sizeStruct);
            return buffer;

        }
        public Byte[] CreateValvePositionRequestMessage()
        {
            buffer = new Byte[sizeStruct];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.ReadValvePosition;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = 0;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, sizeStruct);
            return buffer;

        }
        public Byte[] CreateFansStatusRequestMessage()
        {
            buffer = new Byte[sizeStruct];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.ReadFansStatus;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = 0;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, sizeStruct);
            return buffer;

        }
        public Byte[] GetSingleTaskCommandArrayBytes(SingleTaskMessage pm)
        {
       
            Byte[] locabBuffer = new Byte[sizeStruct];
   

            singleTaskMessage = pm;
            IntPtr pnt = Marshal.AllocHGlobal(sizeStruct);

            try
            {

                // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(singleTaskMessage, pnt, false);

          

                Marshal.Copy(pnt, locabBuffer, 0, sizeStruct);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }
            return locabBuffer;

        }
        private Byte[] GetFullBufferCommandArrayBytes(Full64BufferMessage pm)
        {
            Byte[] locabBuffer = new Byte[64];
            message64 = pm;
            IntPtr pnt = Marshal.AllocHGlobal(64);

            try
            {

                // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(message64, pnt, false);

           

                Marshal.Copy(pnt, locabBuffer, 0, 64);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }
            return locabBuffer;

        }
    }
}
