using EasyLog;
using EasyLog.Factory;
using EasyLog.Interfaces;
using EasyLog.Models;
using EasySave.Factory;
using EasySave.Interfaces;
using EasySave.Models;

namespace EasySave.Managers
{
    class BackupManager
    {
        //Attributs paramètre des sauvegardes
        private Config config;
        private StateManager stateManager;
        private ConfigManager configManager;
        private BackupStrategyFactory backupStrategyFactory;
        private Logger logger;

        /// <summary>
        /// Constructeur
        /// </summary>
        public BackupManager()
        {
            configManager = new ConfigManager();
            stateManager = new StateManager();
            backupStrategyFactory = new BackupStrategyFactory();
            config = configManager.Load();
            LanguageManager.Instance.SetLanguage(config.language.ToString());
            InitializeLogger();
        }

        private void InitializeLogger()
        {
            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySaveData", "Logs");
            ILogFormatter formatter = LogFormatterFactory.Create(config.logType.ToString());
            logger = new Logger(logDirectory, formatter);
        }

        public void SetLanguage(string language)
        {
            switch (language.ToLower())
            {
                case "fr":
                    config.language = Language.FR;
                    break;
                default:
                    config.language = Language.EN;
                    break;
            }
            LanguageManager.Instance.SetLanguage(language);
            configManager.Save(config);
        }

        public void SetLog(string logType)
        {
            switch (logType.ToLower())
            {
                case "xml":
                    config.logType = LogType.XML;
                    break;
                default:
                    config.logType = LogType.JSON;
                    break;
            }
            configManager.Save(config);
            InitializeLogger();
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
            if (config.backupJobs.Count >= 5)
            {
                return false;
            }

            //Création du travailleur
            IBackupStrategy strategy = backupStrategyFactory.Create(backupStrategy);
            config.backupJobs.Add(new BackupJob(name, sourcePath, targetPath, strategy, backupStrategy));

            //Sauvegarde de la configuration du travailleur
            configManager.Save(config);
            return true;
        }

        /// <summary>
        /// Suppresion d'un travailleur de sauvegarde
        /// </summary>
        /// <param name="index"></param>
        public void DeleteJob(int index)
        {
            config.backupJobs.RemoveAt(index);
            configManager.Save(config);
        }

        /// <summary>
        /// Modifie ses chemins
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        public void ModifyJob(int index, string sourcePath, string targetPath)
        {
            config.backupJobs[index].UpdatePaths(sourcePath, targetPath);
            configManager.Save(config);
        }

        /// <summary>
        /// Choix du travailleur à executer
        /// </summary>
        /// <param name="index"></param>
        public void ExecuteJob(int index)
        {
            config.backupJobs[index].Execute(OnProgressUpdate, logger);
        }

        /// <summary>
        /// Liste les travailleurs existants
        /// </summary>
        /// <returns></returns>
        public List<BackupJob> ListJobs()
        {
            return config.backupJobs;
        }

        /// <summary>
        /// Mise à jour de l'état du travailleur
        /// </summary>
        private void OnProgressUpdate()
        {
            stateManager.Write(config.backupJobs);
        }
    }
}
