using System;
using System.IO;
using Newtonsoft.Json;

namespace EasyLog
{
    public class Logger
    {
        //Attribut du dossier log
        private readonly string _logDirectory;


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="logDirectory"></param>
        public Logger(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(_logDirectory);
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
                $"{DateTime.Now:yyyy-MM-dd}.json");

            entry.Timestamp = DateTime.Now;

            // Sérialisation Json
            string json = JsonConvert.SerializeObject(
                entry,
                Formatting.Indented 
            );

            File.AppendAllText(filePath, json + Environment.NewLine);
        }
    }
}
