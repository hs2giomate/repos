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
        public Byte[] CreateCommandFlapperValveMessage(Byte p)
        {
            var size = Marshal.SizeOf(singleTaskMessage);
            buffer = new Byte[size];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.CommandFlapperValve;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = p;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, size);
            return buffer;

        }
        public Byte[] CreateSetpointtFlapperValveMessage(Byte p)
        {

            var size = Marshal.SizeOf(singleTaskMessage);
            buffer = new Byte[size];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.SetPointFlapperValve;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = p;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, size);
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
          //  var size = Marshal.SizeOf(singleTaskMessage);
            Byte[] locabBuffer = new Byte[sizeStruct];
          //  locabBuffer.Initialize();

            singleTaskMessage = pm;
            IntPtr pnt = Marshal.AllocHGlobal(sizeStruct);

            try
            {

                // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(singleTaskMessage, pnt, false);

                // Create another point.
                //  SingleTaskCommand anotherOrder;

                // Set this Point to the value of the
                // Point in unmanaged memory.
                // anotherOrder = (SingleTaskCommand)Marshal.PtrToStructure(pnt, typeof(SingleTaskCommand));

                Marshal.Copy(pnt, locabBuffer, 0, sizeStruct);
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
