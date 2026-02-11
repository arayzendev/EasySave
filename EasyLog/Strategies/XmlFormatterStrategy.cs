using EasyLog.Interfaces;
using System.Xml.Linq;

namespace EasyLog
{
    public class XmlFormatterStrategy : ILogFormatter
    {
        public string FileExtension => "xml";

        public string Format(LogEntry entry)
        {
            var root = new XElement("LogEntry");
            
            root.Add(new XElement("Timestamp", entry.Timestamp?.ToString("O")));
            root.Add(new XElement("Application", entry.Application));
            
            var dataElement = new XElement("Data");
            if (entry.data != null)
            {
                foreach (var kvp in entry.data)
                {
                    dataElement.Add(new XElement(kvp.Key, kvp.Value?.ToString()));
                }
            }
            root.Add(dataElement);
            
            return root.ToString();
        }
    }
}
