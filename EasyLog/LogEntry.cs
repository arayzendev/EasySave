using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog
{
    // Format Log
    public class LogEntry
    {
        public DateTime? Timestamp { get; set; }
        public string Application { get; set; }
        public Dictionary<string, object> data { get; set; }
    }
}
