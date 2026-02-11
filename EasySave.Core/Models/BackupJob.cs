using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using System;
using System.Text.Json.Serialization;

public class BackupJob {

    //Attributs du BackupJKob
    public string name{get; set;}
    public string sourcePath{get; set;}
    public string targetPath{get; set;}
    public string strategyType{get; set;}
    [JsonIgnore]
    public IBackupStrategy backupStrategy{get; set;}
    public BackupProgress backupProgress{get; set;}

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public BackupJob()
    {
        strategyType = "full";
        backupProgress = new BackupProgress();
    }
    /// <summary>
    /// Constructeur paramétré
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    /// <param name="backupStrategy"></param>
    /// <param name="strategyType"></param>
    public BackupJob(string name, string sourcePath, string targetPath, IBackupStrategy backupStrategy, string strategyType)
    {
        this.name=name;
        this.sourcePath=sourcePath;
        this.targetPath=targetPath;
        this.backupStrategy=backupStrategy;
        this.strategyType=strategyType;
        backupProgress = new BackupProgress();
    }

    /// <summary>
    /// Execution d'une strategie de sauvegarde
    /// </summary>
    /// <param name="onProgressUpdate"></param>
    public void Execute(Action onProgressUpdate, EasyLog.Logger logger)
    {
        backupStrategy.Save(sourcePath, targetPath, backupProgress, onProgressUpdate, logger);

    }

    /// <summary>
    /// Mise à jour des chemins
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    public void UpdatePaths(string sourcePath, string targetPath)
    {
        this.sourcePath = sourcePath;
        this.targetPath=targetPath;
    }
}