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
    public struct MessageHeader
    {
      public  Byte command;
       public Byte selector;
       public UInt16 dataSize;
    }

    public class Maintenance_Tool
    {
        MessageHeader header { get;set; }
        public Byte[] buffer;
        public Byte[] tempArray;
        private static Object singletonCreationLock = new Object();
        private static Maintenance_Tool handler;
        public Maintenance_Tool()
        {
          
            buffer = new Byte[64];
        }
        public static void CreateNewEventHandlerForDevice()
        {
            handler = new Maintenance_Tool();
        }
        /// <summary>
        /// Enforces the singleton pattern so that there is only one object handling app events
        /// as it relates to the SerialDevice because this sample app only supports communicating with one device at a time. 
        ///
        /// An instance of EventHandlerForDevice is globally available because the device needs to persist across scenario pages.
        ///
        /// If there is no instance of EventHandlerForDevice created before this property is called,
        /// an EventHandlerForDevice will be created.
        /// </summary>
        public static Maintenance_Tool Current
        {
            get
            {
                if (handler == null)
                {
                    lock (singletonCreationLock)
                    {
                        if (handler == null)
                        {
                            CreateNewEventHandlerForDevice();
                        }
                    }
                }

                return handler;
            }
        }
        public Byte[] CreateMagicArray()
        {
            tempArray = new Byte[4];
            tempArray.Initialize();
            UInt32 tempByte;
            for (int i = 0; i < sizeof(UInt32); i++)
            {
                tempByte =Constants.magic >> (8 * ( sizeof(UInt32)-i-1));
                tempArray[i] = (Byte)(tempByte & (0xff));
            }
            return tempArray;

        }

        public Byte[] BuilAliveMessage()
        {
            var size = Marshal.SizeOf(typeof(SingleTaskCommand));
            buffer = new Byte[size];
            IntPtr pnt = Marshal.AllocHGlobal(size);
            buffer.Initialize();
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            // datagram.order.task = 'I';
            order.command = Commands.KeepConnected;
            datagram.order = order;
            try
            {
               // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(order, pnt, false);
                                Marshal.Copy(pnt, buffer, 0, size);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }

       
            return buffer;

        }
        private Byte[] ObjectToBytes<T>(T obj)
        {
            tempArray = new Byte[128];
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                tempArray = ms.ToArray();
                return tempArray;
            }
        }
    }
}
