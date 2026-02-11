using System.Text.Json;
using EasySave.Core.Factory;
using EasySave.Core.Models;
using EasyLog.Models;

namespace EasySave.Core.Managers
{
    class Config
    {

        public List<BackupJob> backupJobs { get; set; } = new List<BackupJob>();
        public Language language { get; set; } = Language.EN;
        public LogType logType { get; set; } = LogType.JSON;
    }

    class ConfigManager
    {
        //Attribut du chemin d'accï¿½s
        private string filePath;

        /// <summary>
        ///Crï¿½er le dossier de configuration avec son fichier de config
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
        public Config Load()
        {
            //Vï¿½rifie si un fichier existe
            if (!File.Exists(filePath))
            {
                return new Config();
            }

            string json = File.ReadAllText(filePath);

            //Vï¿½rifie si un json est vide
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Config();
            }

            //Dï¿½sï¿½rialisation du fichier
            Config? config = JsonSerializer.Deserialize<Config>(json);
            if (config == null) return new Config();

            //Crï¿½er une stratï¿½gie pour chaque job existant
            BackupStrategyFactory factory = new BackupStrategyFactory();
            foreach (var job in config.backupJobs)
            {
                job.backupStrategy = factory.Create(job.strategyType);
            }

            return config;
        }

        /// <summary>
        /// Sauvegarde la configuration
        /// </summary>
        /// <param name="backupJobs"></param>
        public void Save(Config config)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        }
    } 
}