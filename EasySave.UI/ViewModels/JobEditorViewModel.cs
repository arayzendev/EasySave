using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.GUI.Commands;
using EasySave.Core.Models;

namespace EasySave.GUI.ViewModels
{
    public class JobEditorViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly BackupManager _backupManager;
        private readonly LanguageManager _lang = LanguageManager.Instance;
        private readonly int _index;
        private readonly bool _isModification;

        private string _jobName, _sourcePath, _destinationPath, _encryptionKey, _notifyMsg, _notifyColor;
        private bool _isFullBackup = true, _canSaveQuota = true, _isQuotaAlertVisible, _isNotifyVisible;

        // --- PROPRIÉTÉS DE TRADUCTION ---
       
        public string TitleText => _isModification ? _lang.GetText("Btn_Edit") : _lang.GetText("Menu_Create");
        public string NameLabel => _lang.GetText("Label_Name");
        public string LabelSource => _lang.GetText("Label_Source");
        public string LabelTarget => _lang.GetText("Label_Target");
        public string LabelCrypto => _lang.GetText("Label_Crypto");
        public string LabelStrategy => _lang.GetText("Label_Strategy");

        public string DescName => _lang.GetText("Desc_JobName");
        public string DescSource => _lang.GetText("Desc_Source");
        public string DescTarget => _lang.GetText("Desc_Target");
        public string DescCrypto => _lang.GetText("Desc_Crypto");
        public string DescStrategy => _lang.GetText("Desc_Strategy");

        public string StrategyFull => _lang.GetText("Strategy_Full");
        public string StrategyDiff => _lang.GetText("Strategy_Diff");
        public string SaveBtnText => _lang.GetText("Btn_Validate");
        public string BtnCancel => _lang.GetText("Btn_Cancel");
        public string QuotaMessage => _lang.GetText("Msg_QuotaFull");

        // --- LOGIQUE ET BINDINGS ---
        public bool CanExecuteSave => !string.IsNullOrWhiteSpace(JobName) &&
                                     !string.IsNullOrWhiteSpace(SourcePath) &&
                                     !string.IsNullOrWhiteSpace(DestinationPath) &&
                                     _canSaveQuota;

        public string JobName { get => _jobName; set { _jobName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanExecuteSave)); } }
        public string SourcePath { get => _sourcePath; set { _sourcePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanExecuteSave)); } }
        public string DestinationPath { get => _destinationPath; set { _destinationPath = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanExecuteSave)); } }
        public string EncryptionKey { get => _encryptionKey; set { _encryptionKey = value; OnPropertyChanged(); } }
        public bool IsFullBackup { get => _isFullBackup; set { _isFullBackup = value; OnPropertyChanged(); } }
        public bool IsCreationMode => !_isModification;
        public bool IsQuotaAlertVisible { get => _isQuotaAlertVisible; set { _isQuotaAlertVisible = value; OnPropertyChanged(); } }
        public bool IsNotifyVisible { get => _isNotifyVisible; set { _isNotifyVisible = value; OnPropertyChanged(); } }
        public string NotifyMsg { get => _notifyMsg; set { _notifyMsg = value; OnPropertyChanged(); } }
        public string NotifyColor { get => _notifyColor; set { _notifyColor = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseDestinationCommand { get; }

        public JobEditorViewModel(MainWindowViewModel navigation, BackupJob job = null, int index = -1)
        {
            _navigation = navigation;
            _backupManager = BackupManager.Instance;
            _index = index;
            _isModification = (job != null);

            if (_isModification && job != null)
            {
                JobName = job.name;
                SourcePath = job.sourcePath;
                DestinationPath = job.targetPath;
                IsFullBackup = job.strategyType.ToLower().Contains("full") || job.strategyType.ToLower().Contains("complète");
                EncryptionKey = job.encryptionKey;
            }
            else if (_backupManager.ListJobs().Count >= 5)
            {
                _canSaveQuota = false;
                IsQuotaAlertVisible = true;
            }

            BackCommand = new RelayCommand(() => _navigation.CurrentPage = new DashboardViewModel(_navigation));

            SaveCommand = new AsyncRelayCommand(async () => {
                if (_isModification)
                {
                    _backupManager.ModifyJob(_index, SourcePath, DestinationPath);
                    
                    NotifyMsg = _lang.GetText("Msg_Succes");
                    NotifyColor = "#3498db";
                }
                else
                {
                    if (_backupManager.ListJobs().Any(j => j.name == JobName))
                    {
                        NotifyMsg = _lang.GetText("Msg_ErrorDuplicateName") ?? "A backup with this name already exists";
                        NotifyColor = "#e74c3c";
                        IsNotifyVisible = true;
                        return;
                    }
                    string strategy = IsFullBackup ? "full" : "differential";
                    _backupManager.CreateJob(JobName, SourcePath, DestinationPath, strategy, EncryptionKey);
                    
                    NotifyMsg = _lang.GetText("Msg_Succes");
                    NotifyColor = "#27ae60";
                }

                IsNotifyVisible = true;
                await Task.Delay(1200);
                _navigation.CurrentPage = new DashboardViewModel(_navigation);
            });

            BrowseSourceCommand = new AsyncRelayCommand(async () => await BrowseFolder(true));
            BrowseDestinationCommand = new AsyncRelayCommand(async () => await BrowseFolder(false));
        }

        private async Task BrowseFolder(bool isSource)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var storage = desktop.MainWindow?.StorageProvider;
                if (storage == null) return;

                var result = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = isSource ? LabelSource : LabelTarget,
                    AllowMultiple = false
                });

                if (result != null && result.Count > 0)
                {
                    if (isSource) SourcePath = result[0].Path.LocalPath;
                    else DestinationPath = result[0].Path.LocalPath;
                }
            }
        }
    }
}