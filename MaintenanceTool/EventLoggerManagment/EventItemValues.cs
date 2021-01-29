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
        public string Datetime_String { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        private static int EventNumber = 0;
        private string[] cuttings;
        public EventItemValues(string m)
        {
            Datetime_String = DateTime.Now.ToString();
            EventName = m;
            Description = EventNumber.ToString();
            EventNumber++;

        }

        public void CreateTimeStamp(DataLogItem dli)
        {

            Datetime_String = DateTimeOffset.FromUnixTimeSeconds(dli.timestamp).ToString();
            cuttings = Datetime_String.Split("+");
            Datetime_String = cuttings[0];
            EventName = dli.message;
            Description = EventNumber.ToString();
            EventNumber++;

        }
    }
}
