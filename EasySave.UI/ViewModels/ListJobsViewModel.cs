using System.Collections.ObjectModel;
using System.Windows.Input;
using EasySave.GUI.Commands;
using EasySave.Core.Managers;
using EasySave.Core.Models;

namespace EasySave.GUI.ViewModels
{
    public class ListJobsViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly BackupManager _backupManager;
        private readonly LanguageManager _lang = LanguageManager.Instance;

        // Propriétés de traduction
        public string TitleText => _lang.GetText("Menu_List");
        public string SourceLabel => _lang.GetText("Saisie_Source") ?? "Source :";
        public string TargetLabel => _lang.GetText("Saisie_Dest") ?? "Destination :";

        public ObservableCollection<BackupJob> Jobs { get; set; }
        public ICommand BackCommand { get; }

        public ListJobsViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _backupManager = new BackupManager();

            // On récupère la liste réelle depuis le Core
            var list = _backupManager.ListJobs();
            Jobs = new ObservableCollection<BackupJob>(list);

            BackCommand = new RelayCommand(() => _navigation.CurrentPage = new MainMenuViewModel(_navigation));
        }
    }
}