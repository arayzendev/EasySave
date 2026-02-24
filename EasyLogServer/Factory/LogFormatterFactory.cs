using EasyLog.Interfaces;
using EasyLog.Strategies;


namespace EasyLog.Factory
{
    public static class LogFormatterFactory
    {
        public static ILogFormatter Create(string formatType)
        {
            switch (formatType.ToUpper())
            {
                case "XML":
                    return new XmlFormatterStrategy();
                case "JSON":
                default:
                    return new JsonFormatterStrategy();
            }
        }
    }
}
