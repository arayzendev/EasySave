using EasySave.Factory;
using EasySave.Interfaces;
using EasySave.Models;
using EasySave.Strategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

class BackupManager {
    private List<BackupJob>? backupJobs;
    private StateManager stateManager;
    private ConfigManager configManager;
    private BackupStrategyFactory backupStrategyFactory;

    public BackupManager()
    {
        configManager = new ConfigManager();
        stateManager = new StateManager();
        backupStrategyFactory = new BackupStrategyFactory();
        backupJobs = configManager.Load();
    }
    public bool CreateJob(string name, string sourcePath, string targetPath, string backupStrategy)
    {
        if (backupJobs.Count >= 5)
        {
            return false;
        }

        IBackupStrategy strategy = backupStrategyFactory.Create(backupStrategy);
        backupJobs.Add(new BackupJob(name,sourcePath,targetPath,strategy,backupStrategy));
        configManager.Save(backupJobs);
        return true;
    }
    public void DeleteJob(int index)
    {
        backupJobs.RemoveAt(index);
        configManager.Save(backupJobs);
    }
    public void ModifyJob(int index, string sourcePath, string targetPath)
    {
        backupJobs[index].UpdatePaths(sourcePath,targetPath);
        configManager.Save(backupJobs);
    }
    public void ExecuteJob(int index)
    {
        backupJobs[index].Execute(OnProgressUpdate);
    }
    public List<BackupJob> ListJobs()
    {
        return backupJobs;
    }

    private void OnProgressUpdate()
    {
        stateManager.Write(backupJobs);   
    }
}