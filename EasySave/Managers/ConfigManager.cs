using System.Text.Json;
using EasySave.Factory;

enum Language
{
    FR,
    EN
}

enum LogType
{
    JSON,
    XML
}

class Config
{
    public List<BackupJob> backupJobs;
    public Language language;
    public LogType logType;

    public Config()
    {
        this.backupJobs=new List<BackupJob>();
        this.language=Language.EN;
        this.logType=LogType.JSON;
    }
}

class ConfigManager
{
    //Attribut du chemin d'acc�s
    private string filePath;

    /// <summary>
    ///Cr�er le dossier de configuration avec son fichier de config
    /// </summary>
    public ConfigManager()
    {
        string easySaveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasySaveData");

        //V�rifie l'existence du dossier sinon il le cr�e
        if (!Directory.Exists(easySaveFolder))
        {
            Directory.CreateDirectory(easySaveFolder);
        }
        //Cr�er le fichier config
        this.filePath = Path.Combine(easySaveFolder, "config.json");
    }

    /// <summary>
    /// Charge les backups existants
    /// </summary>
    /// <returns></returns>
    public Config Load()
    {
        //V�rifie si un fichier existe
        if (!File.Exists(filePath))
        {
            return new Config();
        }

        string json = File.ReadAllText(filePath);

        //V�rifie si un json est vide
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Config();
        }

        //D�s�rialisation du fichier
        Config? config = JsonSerializer.Deserialize<Config>(json);
        if (config == null) return new Config();

        //Cr�er une strat�gie pour chaque job existant
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