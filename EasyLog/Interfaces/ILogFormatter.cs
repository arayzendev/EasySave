namespace EasyLog.Interfaces
{
    public interface ILogFormatter
    {
        string Format(LogEntry entry);
        string FileExtension { get; }
    }
}
