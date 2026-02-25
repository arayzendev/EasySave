using EasyLog.Models;
using System.IO;
using System.Text.Json;
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

        public static LogEntry DeserializeXml(string xmlLog)
        {
            if (string.IsNullOrWhiteSpace(xmlLog)) return null;

            try
            {
                using var stringReader = new StringReader(xmlLog);
                var serializer = new XmlSerializer(typeof(LogEntry));
                return (LogEntry)serializer.Deserialize(stringReader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XML deserialization error: {ex.Message}");
                return null;
            }
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