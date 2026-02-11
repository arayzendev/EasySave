using System;
using System.IO;
using EasyLog.Interfaces;
using Newtonsoft.Json;

namespace EasyLog
{
    public class Logger
    {
        //Attribut du dossier log
        private readonly string _logDirectory;
        private ILogFormatter _formatter;


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="logDirectory"></param>
        public Logger(string logDirectory, ILogFormatter formatter)
        {
            _logDirectory = logDirectory;
            _formatter = formatter;
            Directory.CreateDirectory(_logDirectory);
        }

        /// <summary>
        /// Change le formatter (JSON/XML)
        /// </summary>
        public void SetFormatter(ILogFormatter formatter)
        {
            _formatter = formatter;
        }

        /// <summary>
        /// Ecrit les logs
        /// </summary>
        /// <param name="entry"></param>
        public void Write(LogEntry entry)
        {
            //Ajout de la date
            string filePath = Path.Combine(
                _logDirectory,
                $"{DateTime.Now:yyyy-MM-dd}.{_formatter.FileExtension}");

            entry.Timestamp = DateTime.Now;

            // Sérialisation selon le formatter
            string formatted = _formatter.Format(entry);

            File.AppendAllText(filePath, formatted + Environment.NewLine);
        }
    }
}
