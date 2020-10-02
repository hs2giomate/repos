using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLoggerManagment
{
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
    }
}
