﻿using System;
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
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CompressorCompleteMessage
    {

        public Byte relay;
        public UInt16 speed;
        public Byte pressure;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataLogAddressMessage
    {
        public SingleTaskCommand header;
        public UInt32 address;
    }


    public class Protocol
    {
        private static Protocol handler;
        private int sizeStruct;
        private SingleTaskMessage singleTaskMessage;
        private SingleTaskCommand singleTaskCommand;
        private Full64BufferMessage message64;
        public Byte[] buffer;
        private Byte[] local_buffer;
        private DataLogAddressMessage dataLogAddress;
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
        public Byte[] CreateEnableScavengeMessage(Byte p)
        {
            var size = Marshal.SizeOf(singleTaskMessage);
            buffer = new Byte[size];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.EnableScavenge;
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
        public Byte[] CreateSetpointFansMessage(Byte[] p)
        {
            buffer = new Byte[64];
            buffer.Initialize();

            message64 = Buffer64BytesHandler.Message64;
            message64.header.task = Commands.SetpointFans;
            local_buffer = new byte[3];
            local_buffer = p;

            var sizeMessage = Marshal.SizeOf(singleTaskCommand);
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(message64.header), 0, buffer, 0, sizeMessage);
            Buffer.BlockCopy(local_buffer, 0, buffer, sizeMessage, 3);
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
            local_buffer = new byte[2];
            local_buffer = p;
         //   Buffer.BlockCopy(local_buffer, 0, message64.content, 0, 2);
            //   datagram.order = order;
            //     SingleTaskMessage m;
            //      m.header = order;
            //    m.description = p;
            var sizeMessage = Marshal.SizeOf(singleTaskCommand);
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(message64.header), 0, buffer, 0,sizeMessage);
            Buffer.BlockCopy(local_buffer, 0, buffer, sizeMessage, 2);
            return buffer;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Byte[] CreateSetpointFlapperValveMessage(Byte[] p)
        {

            buffer = new Byte[64];
            buffer.Initialize();
            message64 = Buffer64BytesHandler.Message64;
            message64.header.task = Commands.SetPointFlapperValve;
            local_buffer = new byte[2];
            local_buffer = p;
            var sizeMessage = Marshal.SizeOf(singleTaskCommand);
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(message64.header), 0, buffer, 0, sizeMessage);
            Buffer.BlockCopy(local_buffer, 0, buffer, sizeMessage, 2);
            return buffer;

        }
        public Byte[] CreateCompressorMessage(Byte[] p)
        {

            buffer = new Byte[64];
            buffer.Initialize();
            message64 = Buffer64BytesHandler.Message64;
            message64.header.task = Commands.EnableCompressor;
            local_buffer = new byte[4];
            local_buffer = p;
            var sizeMessage = Marshal.SizeOf(singleTaskCommand);
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(message64.header), 0, buffer, 0, sizeMessage);
            Buffer.BlockCopy(local_buffer, 0, buffer, sizeMessage, 4);
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
        public Byte[] CreateScavengeStatusRequestMessage()
        {
            buffer = new Byte[sizeStruct];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.ReadStatusScavenge;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = 0;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, sizeStruct);
            return buffer;

        }
        public Byte[] CreateCompressorStatusRequestMessage()
        {
            buffer = new Byte[sizeStruct];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.ReadStatusCompressor;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = 0;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, sizeStruct);
            return buffer;

        }
        public Byte[] CreateDataLogRequestMessage()
        {
            buffer = new Byte[sizeStruct];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.ReadDataLog;
            datagram.order = order;
            SingleTaskMessage m;
            m.header = order;
            m.description = 0;
            buffer.Initialize();
            Buffer.BlockCopy(GetSingleTaskCommandArrayBytes(m), 0, buffer, 0, sizeStruct);
            return buffer;

        }
        public Byte[] CreateDataLogRequestMessage(UInt32 address)
        {
            var size = Marshal.SizeOf(dataLogAddress);
            buffer = new Byte[size];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.ReadDataLog;
            datagram.order = order;
            DataLogAddressMessage m;
            m.header = order;
            m.address = address;
            buffer.Initialize();
            Buffer.BlockCopy(GetDataLogAddressArrayBytes(m), 0, buffer, 0, size);
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
        public Byte[] GetDataLogAddressArrayBytes(DataLogAddressMessage pm)
        {
            var size = Marshal.SizeOf(dataLogAddress);
            Byte[] locabBuffer = new Byte[size];


            dataLogAddress= pm;
            IntPtr pnt = Marshal.AllocHGlobal(size);

            try
            {

                // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(dataLogAddress, pnt, false);



                Marshal.Copy(pnt, locabBuffer, 0, size);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }
            return locabBuffer;

        }
        public Byte[] GetSingleTaskCommandArrayBytes(SingleTaskCommand pm)
        {
            int sizeMessage = Marshal.SizeOf(singleTaskCommand);
            Byte[] localBuffer = new Byte[sizeMessage];
            singleTaskCommand = pm;
            IntPtr pnt = Marshal.AllocHGlobal(sizeMessage);

            try
            {

                // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(singleTaskCommand, pnt, false);


                Marshal.Copy(pnt, localBuffer, 0, sizeMessage);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }
            return localBuffer;

        }
        private Byte[] GetFullBufferCommandArrayBytes(Full64BufferMessage pm)
        {
            Byte[] locabBuffer = new Byte[64];
            message64 = Buffer64BytesHandler.Message64;
            int sizeMessage = Marshal.SizeOf(message64);
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
