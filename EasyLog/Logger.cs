using System;
using System.IO;
using EasyLog.Interfaces;
using EasyLog.Models;
using Newtonsoft.Json;


namespace EasyLog
{
    public class Logger
    {
        //Attribut du dossier log
        private readonly string _logDirectory;
        private ILogFormatter _formatter;
        private ILogDestination _destination;
        private static readonly object stateFileLock = new object();


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="logDirectory"></param>
        public Logger(ILogFormatter formatter, ILogDestination destination)
        {
            _destination = destination;
            _formatter = formatter;
        }

        /// <summary>
        /// Change le formatter (JSON/XML)
        /// </summary>
        public void SetFormatter(ILogFormatter formatter)
        {
            _formatter = formatter;
        }
        public void SetDestination(ILogDestination destination)
        {
            _destination = destination;
        }

        /// <summary>
        /// Ecrit les logs
        /// </summary>
        /// <param name="entry"></param>
        public void Write(LogEntry entry)
        {
            lock (stateFileLock)

                entry.Timestamp = DateTime.Now;

                // Sérialisation selon le formatter
                string formatted = _formatter.Format(entry);

            _destination.Send(formatted, entry);

        }
    }
}

