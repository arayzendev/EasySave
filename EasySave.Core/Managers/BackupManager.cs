using EasyLog;
using EasyLog.Factory;
using EasyLog.Interfaces;
using EasyLog.Models;
using EasySave.Core.Factory;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using System.Diagnostics;

namespace EasySave.Core.Managers
{ 
    public class BackupManager
    {

        //Attributs paramï¿½tre des sauvegardes
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
        /// Crï¿½ation d'un travailleur de sauvegarde
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="backupStrategy"></param>
        /// <returns></returns>
        public bool CreateJob(string name, string sourcePath, string targetPath, string backupStrategy)
        {

            var stopwatch = Stopwatch.StartNew();

            //Crï¿½ation du travailleur
            IBackupStrategy strategy = backupStrategyFactory.Create(backupStrategy);
            config.backupJobs.Add(new BackupJob(name, sourcePath, targetPath, strategy, backupStrategy));

            //Sauvegarde de la configuration du travailleur
            configManager.Save(config);
            stopwatch.Stop();

            //Ecrit les logs 
            logger.Write(new LogEntry
            {
                Timestamp = DateTime.Now,
                Application = "EasySave",
                data = new Dictionary<string, object>
                                {
                                    { "CreateBackupName", name },
                                    { "SourceFile", sourcePath },
                                    { "TargetFile", targetPath },
                                    { "CreationTimeMs", stopwatch.ElapsedMilliseconds }
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
            config.backupJobs.RemoveAt(index);
            configManager.Save(config);
            stopwatch.Stop();

            //Ecrit les logs 
            logger.Write(new LogEntry
            {
                Timestamp = DateTime.Now,
                Application = "EasySave",
                data = new Dictionary<string, object>
                                {
                                    { "DeletedBackupIndex", index },
                                    { "DeleteTimeMs", stopwatch.ElapsedMilliseconds }
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
            config.backupJobs[index].UpdatePaths(sourcePath, targetPath);
            configManager.Save(config);
            stopwatch.Stop();
            //Ecrit les logs 
            logger.Write(new LogEntry
            {
                Timestamp = DateTime.Now,
                Application = "EasySave",
                data = new Dictionary<string, object>
                                {
                                    { "ModifiedBackupIndex", index },
                                    { "SourceFile", sourcePath },
                                    { "TargetFile", targetPath },
                                    { "TransferTimeMs", stopwatch.ElapsedMilliseconds }
                                }
            });
        }

        /// <summary>
        /// Choix du travailleur ï¿½ executer
        /// </summary>
        /// <param name="index"></param>
        public void ExecuteJob(int index)
        {
            var stopwatch = Stopwatch.StartNew();
            config.backupJobs[index].Execute(OnProgressUpdate, logger);
            stopwatch.Stop();

            //Ecrit les logs 
            logger.Write(new LogEntry
            {
                Timestamp = DateTime.Now,
                Application = "EasySave",
                data = new Dictionary<string, object>
                                {
                                    { "ExecutedBackupIndex", index },
                                    { "TransferTimeMs", stopwatch.ElapsedMilliseconds }
                                }
            });
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
        /// Mise ï¿½ jour de l'ï¿½tat du travailleur
        /// </summary>
        private void OnProgressUpdate()
        {
            stateManager.Write(config.backupJobs);
        }
    }
}