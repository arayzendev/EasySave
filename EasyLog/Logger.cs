using System;
using System.IO;
using Newtonsoft.Json;

namespace EasyLog
{
    public class Logger
    {
        private readonly string _logDirectory;

        public Logger(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(_logDirectory);
        }

        public void Write(LogEntry entry)
        {
            string filePath = Path.Combine(
                _logDirectory,
                $"{DateTime.Now:yyyy-MM-dd}.json");

            entry.Timestamp = DateTime.Now;

            // Sérialisation avec Newtonsoft.Json
            string json = JsonConvert.SerializeObject(
                entry,
                Formatting.Indented // Équivalent de WriteIndented
            );

            File.AppendAllText(filePath, json + Environment.NewLine);
        }
    }
}
