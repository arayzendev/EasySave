using EasyLog;
using EasyLog.Factory;
using EasyLog.Interfaces;
using EasyLog.Models;
using EasySave.Core.Factory;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.Managers;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EasySave.Core.Managers
{
    public class BackupManager
    {
        private static BackupManager _instance = null;
        private static readonly object _lock = new object();

        //Attributs paramètre des sauvegardes
        private Config config;
        private StateManager stateManager;
        private ConfigManager configManager;
        private BackupStrategyFactory backupStrategyFactory;
        private Logger logger;
        private ProcessMonitor processMonitor;


        //Barrage passage des threads, True:ouvert, False:fermé
        //Ouvert par defaut
        private static ManualResetEventSlim _priorityBlocker = new ManualResetEventSlim(true);

        //Compteur de fchiers prioritaires, tous jobs confondus
        private static int _priorityFileCount = 0;

        public static BackupManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new BackupManager();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Constructeur privé pour singleton
        /// </summary>
        public BackupManager()
        {
            configManager = new ConfigManager();
            stateManager = new StateManager();
            backupStrategyFactory = new BackupStrategyFactory();
            processMonitor = new ProcessMonitor();
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

        public bool CreateJob(string name, string sourcePath, string targetPath, string backupStrategy, string encryptionKey = null)
        {
            var stopwatch = Stopwatch.StartNew();

            IBackupStrategy strategy = backupStrategyFactory.Create(backupStrategy);
            var job = new BackupJob(name, sourcePath, targetPath, strategy, backupStrategy);
            job.encryptionKey = encryptionKey;
            config.backupJobs.Add(job);

            configManager.Save(config);
            stopwatch.Stop();

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

        public void DeleteJob(int index)
        {
            var stopwatch = Stopwatch.StartNew();
            config.backupJobs.RemoveAt(index);
            configManager.Save(config);
            stopwatch.Stop();

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

        public void ModifyJob(int index, string sourcePath, string targetPath)
        {
            var stopwatch = Stopwatch.StartNew();
            config.backupJobs[index].UpdatePaths(sourcePath, targetPath);
            configManager.Save(config);
            stopwatch.Stop();

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

        public void SetForbiddenSoftware(string softwareName)
        {
            config.forbiddenSoftwareName = softwareName;
            configManager.Save(config);
        }

        public string GetForbiddenSoftware()
        {
            return config.forbiddenSoftwareName;
        }

        public bool IsForbiddenSoftwareRunning()
        {
            return processMonitor.IsRunning(config.forbiddenSoftwareName);
        }

        public void ExecuteJob(int index, string encryptionKey = null)
        {
            var stopwatch = Stopwatch.StartNew();
            if (IsForbiddenSoftwareRunning())
            {
                logger.Write(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Application = config.backupJobs[index].name,
                    data = new Dictionary<string, object>
                    {
                        { "Status", "Blocked" },
                        { "Reason", "Forbidden software running" },
                        { "SoftwareName", config.forbiddenSoftwareName }
                    }
                });
                config.backupJobs[index].backupProgress.State = BackupState.Failed;
                OnProgressUpdate();
                return;
            }

            string key = encryptionKey ?? config.backupJobs[index].encryptionKey;
            config.backupJobs[index].Execute(OnProgressUpdate, logger, key);
            stopwatch.Stop();

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

        public void PauseJob(int index)
        {
            if (index >= 0 && index < config.backupJobs.Count)
            {
                config.backupJobs[index].Pause();
            }
        }

        public void ResumeJob(int index)
        {
            if (index >= 0 && index < config.backupJobs.Count)
            {
                string key = config.backupJobs[index].encryptionKey;
                config.backupJobs[index].Resume(OnProgressUpdate, logger, key);
            }
        }

        public void StopJob(int index)
        {
            if (index >= 0 && index < config.backupJobs.Count)
            {
                config.backupJobs[index].Stop();
            }
        }

        public List<BackupJob> ListJobs()
        {
            return config.backupJobs;
        }

        private void OnProgressUpdate()
        {
            stateManager.Write(config.backupJobs);
        }

        //Gestion des priorités

        public bool IsPriority(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return config.priorityExtensions.Contains(extension);
        }

        public void PrepareTransfer(string filePath)
        {
            if (IsPriority(filePath))
            {
                Interlocked.Increment(ref _priorityFileCount);
                _priorityBlocker.Reset();
            }
            else
            {
                _priorityBlocker.Wait();
            }
        }

        public void CompleteTransfer(string filePath)
        {
            if (IsPriority(filePath))
            {
                if (Interlocked.Decrement(ref _priorityFileCount) <= 0)
                {
                    _priorityBlocker.Set();
                }
            }
        }

        private static ConcurrentDictionary<string, object> _fileLocks = new ConcurrentDictionary<string, object>();

        public void ExecuteWithPriorityControl(string filePath, Action copyAction)
        {
            PrepareTransfer(filePath);

            object fileLock = _fileLocks.GetOrAdd(filePath, new object());

            //On entoure le try/finally avec le lock pour la sécurité
            lock (fileLock)
            {
                try
                {
                    copyAction();
                }
                finally
                {
                    CompleteTransfer(filePath);
                    _fileLocks.TryRemove(filePath, out _);
                }
            }
        }

        public void BlockNonPriorityFiles()
        {
            _priorityBlocker.Reset();
        }

        public List<string> GetPriorityExtensions()
        {
            return config.priorityExtensions;
        }

        public void UpdatePriorityExtensions(List<string> extensions)
        {
            config.priorityExtensions = extensions;
            configManager.Save(config);
        }
    }
}