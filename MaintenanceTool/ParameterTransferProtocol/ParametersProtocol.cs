using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MaintenanceToolProtocol;
using System.Runtime.InteropServices;

namespace ParameterTransferProtocol
{
    public class ParametersProtocol
    {
        private CommandHeader header=new CommandHeader();
        private ParametersMessage parametersMessage;
        public Byte Command { get; set; }
        public UInt32 Arguments { get; set; }
        public Byte index { get; set; }
        public Parameters ParamtersList;
        public Byte[] buffer;
        public Byte[] tempArray;
        public UInt32 checkSum;
        /// <summary>
        /// Used to synchronize threads to avoid multiple instantiations of eventHandlerForDevice.
        /// </summary>
        private static Object singletonCreationLock = new Object();
        private static ParametersProtocol handler;
        public ParametersProtocol()
        {
            buffer = new Byte[MaintenanceToolProtocol.Constants.BufferSize];
            handler = this;
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
        public static ParametersProtocol Current
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

        /// <summary>
        /// Creates a new instance of EventHandlerForDevice, enables auto reconnect, and uses it as the Current instance.
        /// </summary>
        public static void CreateNewEventHandlerForDevice()
        {
            handler = new ParametersProtocol();
        }
     
        public Byte[] CreateReadParametersRequest()
        {
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
           // datagram.order.task = 'I';
            order.task =Commands.ReadParameters;
            datagram.order = order;
            buffer.Initialize();
           Buffer.BlockCopy(datagram.GetOrderArrayBytes(), 0, buffer, 0, buffer.Length);
            return buffer;

        }

        public Byte[] CreateWriteParametersMessage(UserParameters p)
        {
            var size = Marshal.SizeOf(parametersMessage);
            buffer = new Byte[size];
            CommandHeader datagram = new CommandHeader();
            SingleTaskCommand order = datagram.order;
            order.task = Commands.WriteParameters;
            datagram.order = order;
            ParametersMessage m;
            m.header = order;
                m.parameters = p;
            buffer.Initialize();
            Buffer.BlockCopy(GetWriteParametersArrayBytes(m), 0, buffer, 0, size);
            return buffer;

        }
        public Byte[] GetWriteParametersArrayBytes(ParametersMessage pm)
        {
            var size = Marshal.SizeOf(parametersMessage);
            Byte[] locabBuffer = new Byte[size];
            locabBuffer.Initialize();
           
            parametersMessage=pm;
            IntPtr pnt = Marshal.AllocHGlobal(size);

            try
            {

                // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(parametersMessage, pnt, false);

                // Create another point.
                //  SingleTaskCommand anotherOrder;

                // Set this Point to the value of the
                // Point in unmanaged memory.
                // anotherOrder = (SingleTaskCommand)Marshal.PtrToStructure(pnt, typeof(SingleTaskCommand));

                Marshal.Copy(pnt, locabBuffer, 0, size);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }
            return locabBuffer;

        }


        private Byte[] ObjectToBytes<T>(T obj)
        {
            tempArray =new Byte[buffer.Length];
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
