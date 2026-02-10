using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Core.Factory;

class ConfigManager
{
    //Attribut du chemin d'accès
    private string filePath;

    /// <summary>
    ///Créer le dossier de configuration avec son fichier de config
    /// </summary>
    public ConfigManager()
    {
        string easySaveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasySaveData");

        //Vérifie l'existence du dossier sinon il le crée
        if (!Directory.Exists(easySaveFolder))
        {
            Directory.CreateDirectory(easySaveFolder);
        }
        //Créer le fichier config
        this.filePath = Path.Combine(easySaveFolder, "config.json");
    }

    /// <summary>
    /// Charge les backups existants
    /// </summary>
    /// <returns></returns>
    public List<BackupJob> Load()
    {
        //Vérifie si un fichier existe
        if (!File.Exists(filePath))
        {
            return new List<BackupJob>();
        }

        string json = File.ReadAllText(filePath);

        //Vérifie si un json est vide
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<BackupJob>();
        }

        //Désérialisation du fichier
        List<BackupJob>? backupJobs = JsonSerializer.Deserialize<List<BackupJob>>(json);
        if (backupJobs == null) return new List<BackupJob>();

        //Créer une stratégie pour chaque job existant
        BackupStrategyFactory factory = new BackupStrategyFactory();
        foreach (var job in backupJobs)
        {
            job.backupStrategy = factory.Create(job.strategyType);
        }

        return backupJobs;
    }

    /// <summary>
    /// Sauvegarde la configuration
    /// </summary>
    /// <param name="backupJobs"></param>
    public void Save(List<BackupJob> backupJobs)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(backupJobs, new JsonSerializerOptions { WriteIndented = true }));
    }
}