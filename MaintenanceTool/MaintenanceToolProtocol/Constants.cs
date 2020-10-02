using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceToolProtocol
{
   public class Constants
    {
        public const UInt32 magic = 0x61626364;
        public const UInt32 BufferSize = 64;
        private Byte[] magicArray;

        public  Byte[] ConvertMagicArray(UInt32 m)
        {

            UInt32 tempByte;
            for (int i = 0; i < sizeof(UInt32); i++)
            {

                tempByte = magic >> (8 * (i - sizeof(UInt32)));
                magicArray[i] = (Byte)(tempByte & (0xff));
            }
            return magicArray;
        }
    }
}
