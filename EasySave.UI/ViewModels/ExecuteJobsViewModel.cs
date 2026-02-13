using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EasySave.GUI.Commands;
using EasySave.Core.Managers;
using EasySave.Core.Models;

namespace EasySave.GUI.ViewModels
{
    public class ExecuteJobsViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly BackupManager _backupManager;
        private readonly LanguageManager _lang = LanguageManager.Instance;

        private string _selectedStrategy = "full";
        private bool _isExecuting;

        public string TitleText => _lang.GetText("Menu_Execute") ?? "Orchestration";
        public string ExecuteBtnText => _lang.GetText("Btn_Execute") ?? "Lancer";
        public string BackBtnText => _lang.GetText("Btn_Back") ?? "Retour";
        public string StrategyLabel => "Mode :";

        public ObservableCollection<BackupJob> Jobs { get; set; }
        public ObservableCollection<BackupJob> SelectedJobs { get; } = new ObservableCollection<BackupJob>();

        public List<string> Strategies { get; } = new List<string> { "full", "differential" };
        public string SelectedStrategy { get => _selectedStrategy; set { _selectedStrategy = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand ExecuteCommand { get; }

        public ExecuteJobsViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _backupManager = new BackupManager();
            Jobs = new ObservableCollection<BackupJob>(_backupManager.ListJobs());

            BackCommand = new RelayCommand(() => _navigation.CurrentPage = new MainMenuViewModel(_navigation));

            ExecuteCommand = new RelayCommand(async () =>
            {
                if (SelectedJobs.Count > 0 && !_isExecuting)
                {
                    _isExecuting = true;
                    var tasks = new List<Task>();

                    foreach (var job in SelectedJobs.ToList())
                    {
                        // On force la stratégie choisie avant l'exécution
                        job.strategyType = SelectedStrategy;
                        int index = Jobs.IndexOf(job);

                        // Lancement asynchrone pour ne pas bloquer l'UI
                        tasks.Add(Task.Run(() => _backupManager.ExecuteJob(index)));
                    }

                    // Boucle de rafraîchissement des barres de progression
                    _ = Task.Run(async () => {
                        while (_isExecuting)
                        {
                            OnPropertyChanged(nameof(Jobs));
                            await Task.Delay(100);
                        }
                    });

                    await Task.WhenAll(tasks);
                    _isExecuting = false;
                    OnPropertyChanged(nameof(Jobs)); // Mise à jour finale
                }
            });
        }
    }
}