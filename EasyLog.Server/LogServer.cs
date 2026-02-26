using EasyLog.Models;
using EasyLog.Server.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EasyLog.Server.Services
{
    public class LogServer
    {
        private readonly int _port;

        public LogServer(int port) => _port = port;

        /// <summary>
        /// Gère un client TCP de manière synchrone
        /// </summary>
        private void HandleClient(TcpClient client)
        {
            try
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
        /// Démarre le serveur TCP et accepte plusieurs clients
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

                // Crée un thread pour chaque client
                Thread t = new Thread(() => HandleClient(client))
                {
                    IsBackground = true
                };
                t.Start();
            }
        }
    }
}