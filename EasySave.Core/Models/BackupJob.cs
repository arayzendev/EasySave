using EasySave.Core.Interfaces;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;

namespace EasySave.Core.Models
{
    public class BackupJob : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        public string name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(name)); }
        }

        private string _sourcePath;
        public string sourcePath
        {
            get => _sourcePath;
            set { _sourcePath = value; OnPropertyChanged(nameof(sourcePath)); }
        }

        private string _targetPath;
        public string targetPath
        {
            get => _targetPath;
            set { _targetPath = value; OnPropertyChanged(nameof(targetPath)); }
        }

        private string _strategyType;
        public string strategyType
        {
            get => _strategyType;
            set { _strategyType = value; OnPropertyChanged(nameof(strategyType)); }
        }

        private string _encryptionKey;
        public string encryptionKey
        {
            get => _encryptionKey;
            set { _encryptionKey = value; OnPropertyChanged(nameof(encryptionKey)); }
        }

        [JsonIgnore]
        public IBackupStrategy backupStrategy { get; set; }

        private BackupProgress _backupProgress;
        public BackupProgress backupProgress
        {
            get => _backupProgress;
            set { _backupProgress = value; OnPropertyChanged(nameof(backupProgress)); }
        }
        
        [JsonIgnore]
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public BackupJob()
        {
            strategyType = "full";
            backupProgress = new BackupProgress();
            CancellationTokenSource = new CancellationTokenSource();
        }

        public BackupJob(string name, string sourcePath, string targetPath, IBackupStrategy backupStrategy, string strategyType)
        {
            this.name = name;
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
            this.backupStrategy = backupStrategy;
            this.strategyType = strategyType;
            backupProgress = new BackupProgress();
            CancellationTokenSource = new CancellationTokenSource();
        }

        public void Execute(Action onProgressUpdate, EasyLog.Logger logger, string encryptionKey = null)
        {
            CancellationTokenSource = new CancellationTokenSource();
            backupStrategy.Save(sourcePath, targetPath, backupProgress, onProgressUpdate, logger, encryptionKey, CancellationTokenSource.Token);
        }

        public void Pause()
        {
            if (backupProgress.State == BackupState.Active)
            {
                backupProgress.State = BackupState.Paused;
                CancellationTokenSource.Cancel();
            }
        }

        public void Stop()
        {
            backupProgress.State = BackupState.Stopped;
            CancellationTokenSource.Cancel();
        }

        public void Resume(Action onProgressUpdate, EasyLog.Logger logger, string encryptionKey = null)
        {
            if (backupProgress.State == BackupState.Paused)
            {
                CancellationTokenSource = new CancellationTokenSource();
                backupProgress.State = BackupState.Active;
                backupStrategy.Save(sourcePath, targetPath, backupProgress, onProgressUpdate, logger, encryptionKey, CancellationTokenSource.Token);
            }
        }

        public void UpdatePaths(string sourcePath, string targetPath)
        {
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    } 
}
