using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EventLoggerManagment
{

   // [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataLogItem
    {
       
        public UInt32 timestamp;
        public string message;
    }
    public class EventItemValues
    {
        public string Datetime { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        private static int EventNumber = 0;
        public EventItemValues(string m)
        {
            Datetime = DateTime.Now.ToString();
            EventName = m;
            Description = EventNumber.ToString();
            EventNumber++;

        }

        public void CreateTimeStamp(DataLogItem dli)
        {

            Datetime = DateTime.FromFileTimeUtc(dli.timestamp).ToString();
            EventName = dli.message;
            Description = EventNumber.ToString();
            EventNumber++;

        }
    }
}
