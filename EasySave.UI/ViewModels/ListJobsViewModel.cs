using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.GUI.Commands;
using RelayCommand = EasySave.GUI.Commands.RelayCommand;

namespace EasySave.GUI.ViewModels
{
    public class ListJobsViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly BackupManager _backupManager;
        private readonly LanguageManager _lang = LanguageManager.Instance;

        private string _notifyMsg, _notifyColor;
        private bool _isNotifyVisible;
        private ObservableCollection<BackupJob> _jobs;

        public string TitleText => _lang.GetText("Menu_List") ?? "Travaux";
        public ObservableCollection<BackupJob> Jobs { get => _jobs; set { _jobs = value; OnPropertyChanged(); } }
        public bool IsNotifyVisible { get => _isNotifyVisible; set { _isNotifyVisible = value; OnPropertyChanged(); } }
        public string NotifyMsg { get => _notifyMsg; set { _notifyMsg = value; OnPropertyChanged(); } }
        public string NotifyColor { get => _notifyColor; set { _notifyColor = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand EditJobCommand { get; }
        public ICommand DeleteJobCommand { get; }

        public ListJobsViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _backupManager = new BackupManager();
            RefreshList();

            BackCommand = new RelayCommand(() => _navigation.CurrentPage = new MainMenuViewModel(_navigation));
            EditJobCommand = new RelayCommand<BackupJob>(j => _navigation.CurrentPage = new JobEditorViewModel(_navigation, j, Jobs.IndexOf(j)));

            DeleteJobCommand = new RelayCommand<BackupJob>(async j => {
                if (j != null)
                {
                    _backupManager.DeleteJob(Jobs.IndexOf(j));
                    NotifyMsg = _lang.GetText("Msg_Deleted") ?? "Travail supprimé !";
                    NotifyColor = "#e74c3c"; // ROUGE pour suppression
                    IsNotifyVisible = true;
                    RefreshList();
                    await Task.Delay(1500);
                    IsNotifyVisible = false;
                }
            });
        }
        private void RefreshList() => Jobs = new ObservableCollection<BackupJob>(_backupManager.ListJobs());
    }
}