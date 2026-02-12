using EasyLog.Interfaces;
using Newtonsoft.Json;

namespace EasyLog.Strategies
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
