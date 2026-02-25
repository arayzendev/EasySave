using EasyLog.Models;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

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

        public ClientSocket(string host, int port)
        {
            _host = host;
            _port = port;
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
            using var sw = new StringWriter();
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LogEntry));
            serializer.Serialize(sw, entry);
            return sw.ToString();
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