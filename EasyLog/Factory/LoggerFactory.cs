using EasyLog.Interfaces;
using EasyLog.Models;
using EasyLog.Strategies;
using System;
using System.Collections.Generic;
using System.IO;

namespace EasyLog.Factory
{
    public static class LoggerFactory
    {
        public static Logger CreateLogger(LogMode logMode, string logDirectory, ILogFormatter formatter)
        {
            ILogDestination destination = null;
            ClientSocket clientSocket = null;

            // Si Docker ou Composite, on crée le ClientSocket (connexion persistante)
            if (logMode == LogMode.Docker || logMode == LogMode.Composite)
            {
                clientSocket = new ClientSocket(5000);
                clientSocket.Connect();
            }

            switch (logMode)
            {
                case LogMode.Local:
                    destination = new LocalLoggerStrategy(logDirectory, formatter);
                    break;

                case LogMode.Docker:
                    destination = new DockerLoggerStrategy(clientSocket, formatter);
                    break;

                case LogMode.Composite:
                    destination = new CompositeLoggerStrategy(new ILogDestination[]
                    {
                        new LocalLoggerStrategy(logDirectory, formatter),
                        new DockerLoggerStrategy(clientSocket, formatter)
                    });
                    break;

                default:
                    destination = new LocalLoggerStrategy(logDirectory, formatter);
                    break;
            }

            return new Logger(formatter, destination);
        }
    }
}