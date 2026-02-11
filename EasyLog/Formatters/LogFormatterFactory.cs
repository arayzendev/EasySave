namespace EasyLog
{
    public static class LogFormatterFactory
    {
        public static ILogFormatter Create(string formatType)
        {
            switch (formatType.ToUpper())
            {
                case "XML":
                    return new XmlFormatter();
                case "JSON":
                default:
                    return new JsonFormatter();
            }
        }
    }
}
