using EasyLog;
using EasySave.Core.Factory;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.Core.Strategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

public class BackupManager {

    //Attributs paramètre des sauvegardes
    private List<BackupJob>? backupJobs;
    private StateManager stateManager;
    private ConfigManager configManager;
    private BackupStrategyFactory backupStrategyFactory;

    /// <summary>
    /// Constructeur
    /// </summary>
    public BackupManager()
    {
        configManager = new ConfigManager();
        stateManager = new StateManager();
        backupStrategyFactory = new BackupStrategyFactory();
        backupJobs = configManager.Load();
    }

    /// <summary>
    /// Création d'un travailleur de sauvegarde
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    /// <param name="backupStrategy"></param>
    /// <returns></returns>
    public bool CreateJob(string name, string sourcePath, string targetPath, string backupStrategy)
    {
        var stopwatch = Stopwatch.StartNew();
        //Vérifie si on dépasse pas les 5 travailleurs
        if (backupJobs.Count >= 5)
        {
            return false;
        }

        //Création du travailleur
        IBackupStrategy strategy = backupStrategyFactory.Create(backupStrategy);
        backupJobs.Add(new BackupJob(name,sourcePath,targetPath,strategy,backupStrategy));

        //Sauvegarde de la configuration du travailleur
        configManager.Save(backupJobs);
        //Ecrit les logs 
        Logger logger = new Logger(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySaveData", "Logs"));
        logger.Write(new LogEntry
        {
            Timestamp = DateTime.Now,
            Application = "EasySave",
            data = new Dictionary<string, object>
                            {
                                { "SourceFile", sourcePath },
                                { "TargetFile", targetPath },
                                { "CreateTimeMs", stopwatch.ElapsedMilliseconds },
                                { "Backup travailleur Créé", name}
                            }
        });
        return true;
    }

    /// <summary>
    /// Suppresion d'un travailleur de sauvegarde
    /// </summary>
    /// <param name="index"></param>
    public void DeleteJob(int index)
    {
        var stopwatch = Stopwatch.StartNew();
        backupJobs.RemoveAt(index);
        configManager.Save(backupJobs);
        //Ecrit les logs 
        Logger logger = new Logger(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySaveData", "Logs"));
        logger.Write(new LogEntry
        {
            Timestamp = DateTime.Now,
            Application = "EasySave",
            data = new Dictionary<string, object>
                            {
                                { "Index du Backup travailleur Supprimé", index },
                                { "DeleteTimeMs ", stopwatch.ElapsedMilliseconds }
            }
        });
    }

    /// <summary>
    /// Modifie ses chemins
    /// </summary>
    /// <param name="index"></param>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    public void ModifyJob(int index, string sourcePath, string targetPath)
    {
        var stopwatch = Stopwatch.StartNew();
        backupJobs[index].UpdatePaths(sourcePath,targetPath);
        configManager.Save(backupJobs);
        //Ecrit les logs 
        Logger logger = new Logger(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySaveData", "Logs"));
        logger.Write(new LogEntry
        {
            Timestamp = DateTime.Now,
            Application = "EasySave",
            data = new Dictionary<string, object>
                            {
                                { "Chemin source modifié", sourcePath },
                                { "Chemin cible modifié", targetPath },
                                { "DeleteTimeMs ", stopwatch.ElapsedMilliseconds }
            }
        });
    }

    /// <summary>
    /// Choix du travailleur à executer
    /// </summary>
    /// <param name="index"></param>
    public void ExecuteJob(int index)
    {
        backupJobs[index].Execute(OnProgressUpdate);
    }

    /// <summary>
    /// Liste les travailleurs existants
    /// </summary>
    /// <returns></returns>
    public List<BackupJob> ListJobs()
    {
        return backupJobs;
    }

    /// <summary>
    /// Mise à jour de l'état du travailleur
    /// </summary>
    private void OnProgressUpdate()
    {
        stateManager.Write(backupJobs);   
    }
}