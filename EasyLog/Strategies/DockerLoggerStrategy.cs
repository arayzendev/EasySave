using EasyLog.Interfaces;
using EasyLog.Models;

namespace EasyLog.Strategies
{
    /// <summary>
    /// Stratégie d'envoi vers le serveur Docker via ClientSocket
    /// </summary>
    public class DockerLoggerStrategy : ILogDestination
    {
        private readonly ClientSocket _clientSocket;
        private readonly ILogFormatter _formatter;

        public DockerLoggerStrategy(ClientSocket clientSocket, ILogFormatter formatter)
        {
            _clientSocket = clientSocket;
            _formatter = formatter;
        }

        public void Send(string formattedMessage, LogEntry entry)
        {
            // Le ClientSocket gère la sérialisation et l'envoi
            _clientSocket.Send(entry, _formatter.FileExtension);
        }
    }
}