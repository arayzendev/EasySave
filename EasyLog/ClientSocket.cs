using EasyLog.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace EasyLog
{
    /// <summary>
    /// Gère une connexion TCP persistante vers le serveur EasyLog
    /// </summary>
    public class ClientSocket : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamWriter _writer;
        private bool _connected = false;

        public ClientSocket(int port)
        {
            _host = GetLocalIPAddress();
            _port = port;
        }

        public static string GetLocalIPAddress()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint.Address.ToString();
        }

        /// <summary>
        /// Connexion persistante
        /// </summary>
        public void Connect()
        {
            if (_connected) return;

            _client = new TcpClient();
            _client.Connect(_host, _port);
            _stream = _client.GetStream();
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
            _connected = true;
        }

        /// <summary>
        /// Envoie un log en JSON ou XML via la connexion persistante
        /// </summary>
        public void Send(LogEntry entry, string format)
        {
            if (!_connected)
                Connect();

            string payload = format.ToLower() switch
            {
                "json" => System.Text.Json.JsonSerializer.Serialize(entry),
                "xml" => SerializeXml(entry),
                _ => throw new NotSupportedException("Format non supporté")
            };

            string message = $"{format.ToUpper()}|{payload}";
            _writer.WriteLine(message);
        }

        private string SerializeXml(LogEntry entry)
        {
            var root = new XElement("LogEntry");

            root.Add(new XElement("Timestamp", entry.Timestamp?.ToString("O")));
            root.Add(new XElement("Application", entry.Application));

            var dataElement = new XElement("Data");
            if (entry.data != null)
            {
                foreach (var kvp in entry.data)
                {
                    dataElement.Add(new XElement(kvp.Key, kvp.Value?.ToString()));
                }
            }
            root.Add(dataElement);

            // ToString avec DisableFormatting → tout sur une seule ligne
            return root.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Fermeture propre de la connexion TCP
        /// </summary>
        public void Disconnect()
        {
            if (!_connected) return;

            _writer?.Dispose();
            _stream?.Dispose();
            _client?.Close();
            _connected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}