namespace EasyLog
{
    public interface ILogFormatter
    {
        string Format(LogEntry entry);
        string FileExtension { get; }
    }
}
