using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using MaintenanceToolProtocol;
using Windows.Foundation;

namespace ParameterTransferProtocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UserParameters
    {
        public Byte flapperValveOffset;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ParametersMessage
    {
        public SingleTaskCommand header;
        public UserParameters parameters;
    }
    public  class ParametersStruct
    {
        public  Byte   Command { get; set; }
        public Byte index { get; set; }
        public Parameters ParamtersList;
        public Byte[] buffer;
        public Byte[] parametersbytesArray;
        public UInt32 checkSum;
        public UserParameters userParameters;
        public  static ParametersMessage message;
        public ParametersStruct()
        {
            buffer = new Byte[Constants.BufferSize];
           
        }
        public Byte[]  CreateParameterArray()
        {

            buffer.Append(Command);
            buffer.Append(index);
            GetParametersBytesArray(Parameters.userValues);
            Buffer.BlockCopy(parametersbytesArray, 0, buffer, 2, parametersbytesArray.Length);
            buffer.Append(Convert.ToByte(checkSum));
            return buffer;

        }
   
        private Byte[] GetParametersBytesArray<T>(T obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                parametersbytesArray = ms.ToArray();
                return parametersbytesArray;
            }
        }
        public UserParameters ConvertBytesToParameters(Byte[] data)
        {
            UserParameters received;
            if (data == null)
                return default(UserParameters) ;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                received = (UserParameters)(obj);
                return received;
            }
        }
        public unsafe UserParameters ConvertBytesParameters(Byte[] bytes)
        {
            ParametersMessage pm = message;
            UserParameters p=userParameters;
            var m = Marshal.SizeOf(message);
            var u = Marshal.SizeOf(userParameters);
              IntPtr pnt = Marshal.AllocHGlobal(m);
            try
            {
                
                Marshal.Copy(bytes, 0, pnt, m);
                pm = (ParametersMessage)Marshal.PtrToStructure(pnt, typeof(ParametersMessage));
                p = (UserParameters)pm.parameters;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            finally 
            {

                Marshal.FreeHGlobal(pnt);
            }
         
            return p;
        }

    }
  

}
