```mermaid
classDiagram
    direction TB

    namespace EasySave {

        class Program {
            +static void Main(string[] args)
        }

        class BackupManager {
            -Config config
            -StateManager stateManager
            -ConfigManager configManager
            -BackupStrategyFactory backupStrategyFactory
            -Logger logger
            +BackupManager()
            +void SetLanguage(string language)
            +void SetLog(string logType)
            +bool CreateJob(string name, string sourcePath, string targetPath, string backupStrategy)
            +void DeleteJob(int index)
            +void ModifyJob(int index, string sourcePath, string targetPath)
            +void ExecuteJob(int index)
            +List~BackupJob~ ListJobs()
        }

        class BackupStrategyFactory {
            +BackupStrategyFactory()
            +IBackupStrategy Create(string backupStrategy)
        }

        class StateManager {
            -string filePath
            +StateManager()
            +void Write(List~BackupJob~ backupJobs)
        }

        class ConfigManager {
            -string filePath
            +ConfigManager()
            +Config Load()
            +void Save(Config config)
        }

        class Config {
            +List~BackupJob~ backupJobs
            +Language language
            +LogType logType
        }

        class LanguageManager {
            -static LanguageManager _instance
            -Dictionary~string, string~ _translations
            +string CurrentLanguage
            +static LanguageManager Instance
            +void SetLanguage(string langue)
            +string GetText(string key)
        }

        class Language {
            <<enumeration>>
            FR
            EN
        }

        class BackupJob {
            +string name
            +string sourcePath
            +string targetPath
            +string strategyType
            -IBackupStrategy backupStrategy
            +BackupProgress backupProgress
            +BackupJob()
            +BackupJob(string name, string sourcePath, string targetPath, IBackupStrategy backupStrategy, string strategyType)
            +void Execute(Action onProgressUpdate, Logger logger)
            +void UpdatePaths(string sourcePath, string targetPath)
        }

        class BackupProgress {
            +DateTime? DateTime
            +BackupState State
            +int TotalFiles
            +long TotalSize
            +long FileSize
            +float Progress
            +float TransferTime
            +int RemainingFiles
            +long RemainingSize
            +string SourceFilePath
            +string TargetFilePath
        }

        class BackupState {
            <<enumeration>>
            Active
            inactive
            Ended
            Failed
        }

        class IBackupStrategy {
            <<interface>>
            +void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action onProgressUpdate, Logger logger)
        }

        class FullBackupStrategy {
            +FullBackupStrategy()
            +void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate, Logger logger)
        }

        class DifferentialBackupStrategy {
            +DifferentialBackupStrategy()
            +void Save(string sourcePath, string targetPath, BackupProgress backupProgress, Action OnProgressupdate, Logger logger)
        }
    }


    namespace EasyLog.dll {

        class LogEntry {
            +DateTime? Timestamp
            +string Application
            +Dictionary~string, object~ data
        }

        class Logger {
            -string _logDirectory
            -ILogFormatter _formatter
            +Logger(string logDirectory, ILogFormatter formatter)
            +void SetFormatter(ILogFormatter formatter)
            +void Write(LogEntry entry)
        }

        class ILogFormatter {
            <<interface>>
            +string Format(LogEntry entry)
            +string FileExtension { get; }
        }

        class JsonFormatterStrategy {
            +JsonFormatterStrategy()
            +string Format(LogEntry entry)
            +string FileExtension { get; }
        }

        class XmlFormatterStrategy {
            +XmlFormatterStrategy()
            +string Format(LogEntry entry)
            +string FileExtension { get; }
        }

        class LogFormatterFactory {
            +static ILogFormatter Create(string formatType)
        }

        class LogType {
            <<enumeration>>
            JSON
            XML
        }
    }

    Program --> BackupManager : uses

    BackupManager "1" --> "1" Config : manages
    BackupManager "1" --> "0..5" BackupJob : manages
    BackupManager --> BackupStrategyFactory : uses
    BackupManager --> Logger : uses

    BackupStrategyFactory --> IBackupStrategy : creates

    ConfigManager "1" --> "1" Config : loads-saves

    BackupJob "1" --> "1" IBackupStrategy : uses
    BackupJob "1" --> "1" BackupProgress : has

    BackupProgress --> BackupState : uses

    IBackupStrategy <|.. FullBackupStrategy : implements
    IBackupStrategy <|.. DifferentialBackupStrategy : implements

    StateManager "1" --> "*" BackupJob : writes

    BackupManager "1" --> "1" StateManager : uses

    IBackupStrategy ..> Logger : uses

    Logger --> ILogFormatter : uses
    ILogFormatter <|.. JsonFormatterStrategy : implements
    ILogFormatter <|.. XmlFormatterStrategy : implements
    LogFormatterFactory --> ILogFormatter : creates

    Config --> Language : contains
    Config --> LogType : contains

    BackupManager --> LanguageManager : uses
    LanguageManager --> Language : uses
```
