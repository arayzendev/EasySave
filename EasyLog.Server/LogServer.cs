using EasyLog.Models;
using EasyLog.Server.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog.Server.Services
{
    public class LogServer
    {
        private readonly int _port;

        public LogServer(int port) => _port = port;

        /// <summary>
        /// Gère un client TCP en mode asynchrone
        /// </summary>
        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                string clientIp = remoteEndPoint?.Address.ToString();
                int clientPort = remoteEndPoint?.Port ?? 0;

                Console.WriteLine($"Client connected: {clientIp}:{clientPort}");

                using NetworkStream stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    try
                    {
                        int separatorIndex = line.IndexOf('|');
                        if (separatorIndex <= 0) continue;

                        string format = line.Substring(0, separatorIndex);
                        string payload = line.Substring(separatorIndex + 1);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client connection error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }

        /// <summary>
        /// Démarre le serveur TCP en mode asynchrone
        /// </summary>
        public async Task StartAsync()
        {
            TcpListener server = new TcpListener(IPAddress.Any, _port);
            server.Start();

            Console.WriteLine($"EasyLog.Server listening on port {_port}...");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();

                // 🔥 On ne bloque pas : on lance la tâche en arrière-plan
                _ = HandleClientAsync(client);
            }
        }
    }
}