using EasyLog.Models;

namespace EasyLog.Interfaces
{
    public interface ILogDestination
    {
        void Send(string formattedLog,LogEntry entry);
    }
}