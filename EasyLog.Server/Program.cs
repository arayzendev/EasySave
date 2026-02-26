using EasyLog.Server.Services;

namespace EasyLog.Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int port = 5000;
            LogServer server = new LogServer(port);
            await server.StartAsync();
        }
    }
}