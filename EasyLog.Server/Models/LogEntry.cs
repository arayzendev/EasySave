

namespace EasyLog.Models
{
    // Format Log
    public class LogEntry
    {
        public DateTime? Timestamp { get; set; }
        public string Application { get; set; }
        public Dictionary<string, object> data { get; set; }
        public string User { get; set; }
    }
}
