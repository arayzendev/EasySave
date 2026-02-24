using EasyLog.Models;

namespace EasyLog.Interfaces
{
    public interface ILogger
    {
        void Write(LogEntry entry);
    }
}