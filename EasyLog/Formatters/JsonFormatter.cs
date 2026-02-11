using Newtonsoft.Json;

namespace EasyLog
{
    public class JsonFormatter : ILogFormatter
    {
        public string FileExtension => "json";

        public string Format(LogEntry entry)
        {
            return JsonConvert.SerializeObject(entry, Formatting.Indented);
        }
    }
}
