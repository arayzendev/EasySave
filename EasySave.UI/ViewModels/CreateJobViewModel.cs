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
        private bool _isSuccessMessageVisible;

        
        public string TitleText => _lang.GetText("Menu_Create");
        public string SuccessText => _lang.GetText("Msg_Succes");
        public string PlaceholderName => _lang.GetText("Saisie_Nom");
        public string SourceWatermark => _lang.GetText("Saisie_Source");
        public string TargetWatermark => _lang.GetText("Saisie_Dest");
        public string CancelText => _lang.GetText("Btn_Cancel"); 
        public string ValidateText => _lang.GetText("Btn_Validate"); 

        public string JobName { get => _jobName; set { _jobName = value; OnPropertyChanged(); } }
        public string SourcePath { get => _sourcePath; set { _sourcePath = value; OnPropertyChanged(); } }
        public string DestinationPath { get => _destinationPath; set { _destinationPath = value; OnPropertyChanged(); } }
        public bool IsSuccessMessageVisible { get => _isSuccessMessageVisible; set { _isSuccessMessageVisible = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseDestinationCommand { get; }

        public CreateJobViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _backupManager = new BackupManager();

            CancelCommand = new RelayCommand(() => _navigation.CurrentPage = new MainMenuViewModel(_navigation));

            SaveCommand = new RelayCommand(async () => {
                _backupManager.CreateJob(JobName, SourcePath, DestinationPath, "full");
                IsSuccessMessageVisible = true;
                await Task.Delay(1500);
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