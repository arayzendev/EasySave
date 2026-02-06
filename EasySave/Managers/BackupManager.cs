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
        return true;
    }

    /// <summary>
    /// Suppresion d'un travailleur de sauvegarde
    /// </summary>
    /// <param name="index"></param>
    public void DeleteJob(int index)
    {
        backupJobs.RemoveAt(index);
        configManager.Save(backupJobs);
    }

    /// <summary>
    /// Modifie ses chemins
    /// </summary>
    /// <param name="index"></param>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    public void ModifyJob(int index, string sourcePath, string targetPath)
    {
        backupJobs[index].UpdatePaths(sourcePath,targetPath);
        configManager.Save(backupJobs);
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