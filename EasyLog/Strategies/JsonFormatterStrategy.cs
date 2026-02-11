using EasyLog.Interfaces;
using Newtonsoft.Json;

namespace EasyLog.Models
{
    public class JsonFormatterStrategy : ILogFormatter
    {
        public string FileExtension => "json";

        public string Format(LogEntry entry)
        {
            return JsonConvert.SerializeObject(entry, Formatting.Indented);
        }
    }
}
