using EasySave.Interfaces;
using EasySave.Models;
using System;

class BackupJob {

    private string name;
    public string sourcePath{get; private set;}
    public string targetPath{get; private set;}
    private IBackupStrategy backupStrategy;
    private BackupProgress backupProgress;

    public BackupJob()
    {
        backupProgress = new BackupProgress();
    }
    public BackupJob(string name, string sourcePath, string targetPath, IBackupStrategy backupStrategy)
    {
        this.name=name;
        this.sourcePath=sourcePath;
        this.targetPath=targetPath;
        this.backupStrategy=backupStrategy;
        backupProgress = new BackupProgress();
    }
    public void Execute(Action onProgressUpdate)
    {
        backupStrategy.Save(sourcePath, targetPath, backupProgress, onProgressUpdate);
    }

    public void UpdatePaths(string sourcePath, string targetPath)
    {
        this.sourcePath = sourcePath;
        this.targetPath=targetPath;
    }
}