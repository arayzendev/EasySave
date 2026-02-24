using EasyLog.Interfaces;
using EasyLog.Models;
using EasyLog.Strategies;

namespace EasyLog.Factory
{
    public static class LoggerFactory
    {
        public static List<ILogger> CreateLoggers(LogMode mode, ILogFormatter formatter, string localDir, string dockerHost = "127.0.0.1", int dockerPort = 5001)
        {
            var loggers = new List<ILogger>();

            switch (mode)
            {
                case LogMode.Local:
                    loggers.Add(new LocalLoggerStrategy(localDir, formatter));
                    break;
                case LogMode.Docker:
                    loggers.Add(new DockerLoggerStrategy(formatter, dockerHost, dockerPort));
                    break;
                case LogMode.LocalAndDocker:
                    loggers.Add(new LocalLoggerStrategy(localDir, formatter));
                    loggers.Add(new DockerLoggerStrategy(formatter, dockerHost, dockerPort));
                    break;
            }

            return loggers;
        }
    }
}