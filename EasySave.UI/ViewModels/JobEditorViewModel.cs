using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.GUI.Commands;

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

        public string TitleText => _isModification ? _lang.GetText("Menu_Modify") : _lang.GetText("Menu_Create");
        public string NameLabel => _lang.GetText("Saisie_Nom");
        public string SourceWatermark => _lang.GetText("Saisie_Source");
        public string TargetWatermark => _lang.GetText("Saisie_Dest");
        public string SaveBtnText => _lang.GetText("Btn_Validate");
        public string QuotaMessage => _lang.GetText("Msg_QuotaFull") ?? "Quota 5/5 atteint";
        public string EncryptionLabel => "Clé de chiffrement (optionnel) :";

        public bool CanExecuteSave => !string.IsNullOrWhiteSpace(JobName) && !string.IsNullOrWhiteSpace(SourcePath) && !string.IsNullOrWhiteSpace(DestinationPath) && _canSaveQuota;
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
                JobName = job.name; SourcePath = job.sourcePath; DestinationPath = job.targetPath;
                IsFullBackup = job.strategyType.ToLower() == "full";
                EncryptionKey = job.encryptionKey;
            }
            else if (_backupManager.ListJobs().Count >= 5)
            {
                _canSaveQuota = false; IsQuotaAlertVisible = true;
            }

            // CORRECTION 1 : Retour direct au Dashboard
            BackCommand = new RelayCommand(() => _navigation.CurrentPage = new DashboardViewModel(_navigation));

            SaveCommand = new RelayCommand(async () => {
                if (_isModification)
                {
                    _backupManager.ModifyJob(_index, SourcePath, DestinationPath);
                    NotifyMsg = _lang.GetText("Msg_Modified") ?? "Travail modifié !";
                    NotifyColor = "#3498db";
                }
                else
                {
                    _backupManager.CreateJob(JobName, SourcePath, DestinationPath, IsFullBackup ? "full" : "differential", EncryptionKey);
                    NotifyMsg = _lang.GetText("Msg_Created") ?? "Travail créé !";
                    NotifyColor = "#27ae60";
                }

                IsNotifyVisible = true;
                await Task.Delay(1500);

                // CORRECTION 2 : Après sauvegarde, retour au Dashboard
                _navigation.CurrentPage = new DashboardViewModel(_navigation);
            });

            BrowseSourceCommand = new RelayCommand(async () => await BrowseFolder(true));
            BrowseDestinationCommand = new RelayCommand(async () => await BrowseFolder(false));
        }

        private async Task BrowseFolder(bool isSource)
        {
            var topLevel = TopLevel.GetTopLevel((Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel != null)
            {
                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
                if (folders?.Count > 0)
                {
                    if (isSource) SourcePath = folders[0].Path.LocalPath;
                    else DestinationPath = folders[0].Path.LocalPath;
                }
            }
        }
    }
}