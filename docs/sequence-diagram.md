```mermaid
sequenceDiagram
    autonumber

    actor User
    participant Program
    participant BackupManager
    participant BackupJob
    participant IBackupStrategy
    participant StateManager
    participant Logger

    User->>Program: Run EasySave.exe 1
    Program->>BackupManager: ExecuteJob(1)
    
    BackupManager->>BackupJob: Execute(onProgressUpdate)
    BackupJob->>IBackupStrategy: Save(sourcePath, targetPath, backupProgress, onProgressUpdate)

    loop For each file
        IBackupStrategy->>Logger: Write(data)
        IBackupStrategy->>BackupManager: onProgressUpdate()
        BackupManager->>StateManager: Write(backupJobs)
    end
    BackupManager-->>Program: Success
```