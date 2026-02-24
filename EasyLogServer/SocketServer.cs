using EasyLog.Factory;
using EasyLog.Interfaces;
using EasyLog.Models;
using EasyLog.Strategies;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace EasyLog
{
    public class LogPackage
    {
        public LogEntry Entry { get; set; }
        public LogType Format { get; set; }
        public LogMode Mode { get; set; }
    }

    public class SocketServer
    {
        private readonly TcpListener _listener;

        // Centralisation des loggers par format pour éviter de recréer chaque fois
        private readonly Dictionary<LogType, ILogFormatter> _formatters = new();
        private readonly Dictionary<LogMode, ILogger> _loggers = new();

        public SocketServer(int port = 5001)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            Console.WriteLine($"EasyLog Server started on port {port}");
            Task.Run(AcceptClients);
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (line == null) break;

                try
                {
                    // Désérialisation du package envoyé par le client
                    var package = JsonConvert.DeserializeObject<LogPackage>(line);
                    if (package == null || package.Entry == null) continue;

                    // Récupère ou crée le formatter demandé
                    if (!_formatters.ContainsKey(package.Format))
                        _formatters[package.Format] = LogFormatterFactory.Create(package.Format.ToString());

                    ILogFormatter formatter = _formatters[package.Format];

                    // Récupère ou crée le logger demandé
                    if (!_loggers.ContainsKey(package.Mode))
                        _loggers[package.Mode] = LoggerFactory.CreateLoggers(package.Mode, formatter, );

                    ILogger logger = _loggers[package.Mode];

                    // Écriture du log selon format et mode
                    logger.Write(package.Entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process log: {ex.Message}");
                }
            }
        }

        static void Main(string[] args)
        {
            // Lancement du serveur
            var server = new SocketServer(5001);

            Console.WriteLine("Server running. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}