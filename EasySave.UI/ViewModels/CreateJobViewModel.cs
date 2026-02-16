using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using EasySave.Core.Managers;
using EasySave.GUI.Commands;

namespace EasySave.GUI.ViewModels
{
    public class CreateJobViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly BackupManager _backupManager;
        private readonly LanguageManager _lang = LanguageManager.Instance;

        private string _jobName;
        private string _sourcePath;
        private string _destinationPath;
        private string _encryptionKey;
        private bool _isFullBackup = true;
        private bool _isDifferentialBackup;
        private bool _canCreate = true;
        private bool _isQuotaAlertVisible;
        private bool _isSuccessMessageVisible;

        // --- Traductions (LanguageManager) ---
        public string TitleText => _lang.GetText("Menu_Create");
        public string NameLabel => _lang.GetText("Saisie_Nom");
        public string SourceWatermark => _lang.GetText("Saisie_Source");
        public string TargetWatermark => _lang.GetText("Saisie_Dest");
        public string TypeLabel => _lang.GetText("Saisie_Type") ?? "Stratégie :";
        public string FullText => _lang.GetText("Type_Full") ?? "Complète";
        public string DiffText => _lang.GetText("Type_Diff") ?? "Différentielle";
        public string SaveBtnText => _lang.GetText("Btn_Validate");
        public string CancelBtnText => _lang.GetText("Btn_Cancel");
        public string QuotaMessage => _lang.GetText("Msg_QuotaFull") ?? "Limite de 5 travaux atteinte.";
        public string EncryptionLabel => "Clé de chiffrement (optionnel) :";

        // --- Propriétés Formulaire ---
        public string JobName { get => _jobName; set { _jobName = value; OnPropertyChanged(); } }
        public string SourcePath { get => _sourcePath; set { _sourcePath = value; OnPropertyChanged(); } }
        public string DestinationPath { get => _destinationPath; set { _destinationPath = value; OnPropertyChanged(); } }
        public string EncryptionKey { get => _encryptionKey; set { _encryptionKey = value; OnPropertyChanged(); } }
        public bool IsFullBackup { get => _isFullBackup; set { _isFullBackup = value; OnPropertyChanged(); } }
        public bool IsDifferentialBackup { get => _isDifferentialBackup; set { _isDifferentialBackup = value; OnPropertyChanged(); } }
        public bool CanCreate { get => _canCreate; set { _canCreate = value; OnPropertyChanged(); } }
        public bool IsQuotaAlertVisible { get => _isQuotaAlertVisible; set { _isQuotaAlertVisible = value; OnPropertyChanged(); } }

        // --- Commandes ---
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseDestinationCommand { get; }

        public CreateJobViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _backupManager = new BackupManager();

            // Vérification du Quota 5 dès le début
            var jobs = _backupManager.ListJobs();
            if (jobs != null && jobs.Count >= 5)
            {
                CanCreate = false;
                IsQuotaAlertVisible = true;
            }

            BackCommand = new RelayCommand(() => _navigation.CurrentPage = new MainMenuViewModel(_navigation));

            CancelCommand = new RelayCommand(() => {
                JobName = string.Empty;
                SourcePath = string.Empty;
                DestinationPath = string.Empty;
                IsFullBackup = true;
                IsDifferentialBackup = false;
            });

            SaveCommand = new RelayCommand(async () => {
                if (!CanCreate) return;
                if (string.IsNullOrWhiteSpace(JobName) || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
                {
                    return;
                }
                string strategy = IsFullBackup ? "full" : "differential";
                _backupManager.CreateJob(JobName, SourcePath, DestinationPath, strategy, EncryptionKey);
                _navigation.CurrentPage = new MainMenuViewModel(_navigation);
            });

            BrowseSourceCommand = new RelayCommand(async () => await BrowseFolder(true));
            BrowseDestinationCommand = new RelayCommand(async () => await BrowseFolder(false));
        }

        private async Task BrowseFolder(bool isSource)
        {
            var topLevel = TopLevel.GetTopLevel((Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel != null)
            {
                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = isSource ? SourceWatermark : TargetWatermark,
                    AllowMultiple = false
                });
                if (folders != null && folders.Count > 0)
                {
                    if (isSource) SourcePath = folders[0].Path.LocalPath;
                    else DestinationPath = folders[0].Path.LocalPath;
                }
            }
        }
    }
}