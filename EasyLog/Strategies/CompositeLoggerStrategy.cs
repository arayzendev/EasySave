using EasyLog.Interfaces;
using EasyLog.Models;

namespace EasyLog.Strategies
{
    public class CompositeLoggerStrategy : ILogDestination
    {
        private readonly IEnumerable<ILogDestination> _destinations;

        public CompositeLoggerStrategy(IEnumerable<ILogDestination> destinations)
        {
            _destinations = destinations;
        }

        public void Send(string formattedMessage, LogEntry entry)
        {
            foreach (var destination in _destinations)
            {
                destination.Send(formattedMessage, entry);
            }
        }
    }
}