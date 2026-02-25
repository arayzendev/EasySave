using EasyLog.Models;
using EasyLog.Server.Utils;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyLog.Server.Services
{
    public class LogServer
    {
        private readonly int _port;

        public LogServer(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Gère un client TCP : lecture ligne par ligne, désérialisation et stockage
        /// </summary>
        private void HandleClient(TcpClient client)
        {
            using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    int separatorIndex = line.IndexOf('|');
                    if (separatorIndex <= 0) continue;

                    // Extraction de l'en-tête
                    string format = line.Substring(0, separatorIndex);
                    string payload = line.Substring(separatorIndex + 1); // <-- en-tête supprimée

                    // Désérialisation selon format
                    LogEntry entry = LogDeserializer.Deserialize(format, payload);
                    if (entry != null)
                    {
                        LogStorage.Save(entry, format);
                        Console.WriteLine($"Log saved: {entry.Application} at {entry.Timestamp}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing log: {ex.Message}");
                }
            }

            client.Close();
            Console.WriteLine("Client disconnected.");
        }

        /// <summary>
        /// Démarre le serveur TCP
        /// </summary>
        public void Start()
        {
            TcpListener server = new TcpListener(IPAddress.Any, _port);
            server.Start();
            Console.WriteLine($"EasyLog.Server listening on port {_port}...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                HandleClient(client);
            }
        }
    }
}