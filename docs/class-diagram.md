```mermaid
classDiagram
    direction TB

    %% EasySave.Console - Entry Point
    class Program {
        +Main(args: string[])
    }

    %% EasySave.Core - Services
    class BackupManager {
    }

    class StateManager {
    }

    class ConfigManager {
    }

    class LanguageManager {
    }

    %% EasySave.Core - Models
    class BackupJob {
    }

    class BackupProgress {
    }

    %% EasySave.Core - Strategies
    class IBackupStrategy {
        <<interface>>
    }

    class FullBackup {
    }

    class DifferentialBackup {
    }

    %% EasyLog - Separate DLL
    class Logger {
    }

    %% Relationships
    Program --> BackupManager : uses

    BackupManager "1" --> "0..5" BackupJob : manages
    BackupManager ..> LanguageManager : uses

    BackupJob "1" --> "1" IBackupStrategy : uses
    BackupJob "1" --> "1" BackupProgress : has

    IBackupStrategy <|.. FullBackup : implements
    IBackupStrategy <|.. DifferentialBackup : implements

    StateManager "1" --> "*" BackupProgress : writes
    ConfigManager "1" --> "*" BackupJob : loads-saves

    BackupJob ..> Logger : uses
```