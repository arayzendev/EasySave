using EasyLog.Server.Services;

namespace EasyLog.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int port = 5000; // même que DockerLoggerStrategy
            LogServer server = new LogServer(port);
            server.Start();
        }
    }
}