using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;

namespace ParameterTransferProtocol
{
    public struct FlapperValveData
    {
        public Byte offset { set; get; }
            
        public float angle { set; get; }
    }

    public class Parameters
    {
        public static Parameters userValues;
        public FlapperValveData flapperValveData;

    }
    
}
