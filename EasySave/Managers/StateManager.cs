using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


class StateManager {
    //Attributs chemin d'accès
    private string filePath;

    /// <summary>
    /// Créer le fichier de l'état
    /// </summary>
    public StateManager()
    {
        //Vérifie l'existence du dossier sinon il le crée
        string easySaveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasySaveData");

        if (!Directory.Exists(easySaveFolder))
        {
            Directory.CreateDirectory(easySaveFolder);
        }
        //Créer le fichier d'état
        this.filePath = Path.Combine(easySaveFolder, "state.json");
    }

    /// <summary>
    /// Ecrit dans le json d'état
    /// </summary>
    /// <param name="backupJobs"></param>
    public void Write(List<BackupJob> backupJobs)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(backupJobs, new JsonSerializerOptions { WriteIndented = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }));
    }
}