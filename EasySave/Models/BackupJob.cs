using EasySave.Interfaces;
using EasySave.Models;
using System;
using System.Text.Json.Serialization;

class BackupJob {

    public string name{get; set;}
    public string sourcePath{get; set;}
    public string targetPath{get; set;}
    public string strategyType{get; set;}
    [JsonIgnore]
    public IBackupStrategy backupStrategy{get; set;}
    public BackupProgress backupProgress{get; set;}

    public BackupJob()
    {
        strategyType = "full";
        backupProgress = new BackupProgress();
    }
    public BackupJob(string name, string sourcePath, string targetPath, IBackupStrategy backupStrategy, string strategyType)
    {
        this.name=name;
        this.sourcePath=sourcePath;
        this.targetPath=targetPath;
        this.backupStrategy=backupStrategy;
        this.strategyType=strategyType;
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