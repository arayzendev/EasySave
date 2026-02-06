using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog
{
    public class LogEntry
    {
        public DateTime? Timestamp;
        public string Application;
        public Dictionary<string, object> data;
    }
}
