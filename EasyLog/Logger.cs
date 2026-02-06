using System;
using System.IO;
using System.Text.Json;

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

            string json = JsonSerializer.Serialize(
                entry,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.AppendAllText(filePath, json + Environment.NewLine);
        }
    }
}
