using System;
using System.IO;
using System.Net.Sockets;
using EasyLog.Interfaces;
using EasyLog.Models;

namespace EasyLog.Strategies
{
    public class DockerLoggerStrategy : ILogger
    {
        private readonly ILogFormatter _formatter;
        private readonly string _host;
        private readonly int _port;

        public DockerLoggerStrategy(
            ILogFormatter formatter,
            string dockerHost = "127.0.0.1",
            int dockerPort = 5001)
        {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _host = dockerHost;
            _port = dockerPort;
        }

        public void Write(LogEntry entry)
        {
            entry.Timestamp = DateTime.Now;
            string formatted = _formatter.Format(entry);

            using var client = new TcpClient(_host, _port);
            using var writer = new StreamWriter(client.GetStream())
            {
                AutoFlush = true
            };

            writer.WriteLine(formatted);
        }
    }
}