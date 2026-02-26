using EasyLog.Models;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EasyLog.Server.Utils
{
    public static class LogDeserializer

    {
        public static LogEntry DeserializeJson(string jsonLog)
        {

            if (string.IsNullOrWhiteSpace(jsonLog)) return null;

            try
            {
                return JsonSerializer.Deserialize<LogEntry>(jsonLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                return null;
            }
        }

        public  static LogEntry DeserializeXml(string xmlLog)
        {
            if (string.IsNullOrWhiteSpace(xmlLog))
                return null;

            var root = XElement.Parse(xmlLog);

            var entry = new LogEntry();

            // Timestamp
            var timestampElement = root.Element("Timestamp");
            if (timestampElement != null &&
                DateTime.TryParse(timestampElement.Value, out var ts))
            {
                entry.Timestamp = ts;
            }

            // Application
            entry.Application = root.Element("Application")?.Value;

            // Data
            var dataElement = root.Element("Data");
            if (dataElement != null)
            {
                // ⚠️ IMPORTANT : on s'assure que le dictionnaire est instancié
                entry.data ??= new Dictionary<string, object>();

                foreach (var element in dataElement.Elements())
                {
                    entry.data[element.Name.LocalName] = element.Value;
                }
            }

            return entry;
        }

        public static LogEntry Deserialize(string format, string payload)
        {
            return format.ToUpper() switch
            {
                "JSON" => DeserializeJson(payload),
                "XML" => DeserializeXml(payload),
                _ => null
            };
        }
    }
}