using System;
using System.IO;
using EasyLog.Interfaces;
using EasyLog.Models;
using Newtonsoft.Json;

namespace EasyLog.Strategies
{
    public class LocalLoggerStrategy : ILogDestination
    {
        //Attribut du dossier log
        private readonly string _logDirectory;
        private ILogFormatter _formatter;
        private static readonly object stateFileLock = new object();


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="logDirectory"></param>
        public LocalLoggerStrategy(string logDirectory, ILogFormatter formatter)
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
        public void Send(string formattedLog, LogEntry entry)
        {
            lock (stateFileLock)
            {
                //Ajout de la date
                string filePath = Path.Combine(
                _logDirectory,
                $"{DateTime.Now:yyyy-MM-dd}.{_formatter.FileExtension}");

                entry.Timestamp = DateTime.Now;

                File.AppendAllText(filePath, formattedLog + Environment.NewLine);

            }
        }
    }
}
