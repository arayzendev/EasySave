using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


class StateManager {
    private string filePath;

    public StateManager()
    {
        string easySaveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasySaveData");

        if (!Directory.Exists(easySaveFolder))
        {
            Directory.CreateDirectory(easySaveFolder);
        }

        this.filePath = Path.Combine(easySaveFolder, "state.json");
    }
    public void Write(List<BackupJob> backupJobs)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(backupJobs, new JsonSerializerOptions { WriteIndented = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }));
    }
}