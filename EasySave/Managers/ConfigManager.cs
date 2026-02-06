using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class ConfigManager
{
    private string filePath;

    public ConfigManager()
    {
        string easySaveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasySave");

        if (!Directory.Exists(easySaveFolder))
        {
            Directory.CreateDirectory(easySaveFolder);
        }

        this.filePath = Path.Combine(easySaveFolder, "config.json");
    }
    public List<BackupJob> Load()
    {
        if (!File.Exists(filePath))
        {
            return new List<BackupJob>();
        }

        string json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<BackupJob>();
        }

        List<BackupJob>? backupJobs = JsonSerializer.Deserialize<List<BackupJob>>(json);
        return backupJobs ?? new List<BackupJob>();
    }
    public void Save(List<BackupJob> backupJobs)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(backupJobs, new JsonSerializerOptions { WriteIndented = true }));
    }
}