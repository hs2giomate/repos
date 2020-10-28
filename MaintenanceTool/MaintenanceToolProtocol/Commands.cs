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
    public struct SingleTaskCommand
    {
        public UInt32 magic;
        public char command;
        public char task;
       
    }
    
    public class CommandHeader
    {
        public SingleTaskCommand order;
        public Byte[] bytes;
        public CommandHeader() 
        {
         order.magic = Commands.Magic;
            order.command = Commands.Command;

         }
        public Byte[] GetOrderArrayBytes()
        {
            bytes = new Byte[Constants.BufferSize];
            bytes.Initialize();
            var size = Marshal.SizeOf(order);
            IntPtr pnt = Marshal.AllocHGlobal(size);
            
            try
            {

                // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(order, pnt, false);

                // Create another point.
             //  SingleTaskCommand anotherOrder;

                // Set this Point to the value of the
                // Point in unmanaged memory.
               // anotherOrder = (SingleTaskCommand)Marshal.PtrToStructure(pnt, typeof(SingleTaskCommand));

                Marshal.Copy(pnt, bytes, 0, size);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }
            return bytes;

        }
      


    }
    public class Commands
    {
        public const Byte read = 0x02;
        public const Byte write = 0x06;
        public const char ReadParameters = 'H';
        public const char WriteParameters = 'G';
        public const char EnableHeaters = 'I';
        public const char ReadStatusHeaters = 'J';
        public const char Command = '>';
        public const char KeepConnected= '=';
        public const UInt32 Magic = 0x64636261;
    }
}
