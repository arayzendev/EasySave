using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Managers;
using EasySave.Core.Models;

namespace EasySave.GUI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly BackupManager _backupManager;
        private readonly LanguageManager _lang = LanguageManager.Instance;
        private readonly DispatcherTimer _refreshTimer;

        // Liste pour protéger les jobs du rafraîchissement immédiat de l'état
        private readonly Dictionary<string, DateTime> _protectedJobs = new Dictionary<string, DateTime>();

        private ObservableCollection<BackupJob> _jobs;
        public ObservableCollection<BackupJob> Jobs
        {
            get => _jobs;
            set { _jobs = value; OnPropertyChanged(); UpdateSections(); }
        }

        //PROPRIÉTÉS DE TRADUCTION POUR LE XAML
        public string TitleText => _lang.GetText("Menu_Titre") ?? "--- TABLEAU DE BORD ---";
        public string ActiveSectionText => _lang.GetText("Label_ActiveMissions") ?? "MISSIONS ACTIVES";
        public string HistorySectionText => _lang.GetText("Label_History") ?? "HISTORIQUE";
        public string ExecuteAllBtnText => _lang.GetText("Menu_ExecuteAll") ?? "TOUT EXÉCUTER";
        public string ProgressLabelText => _lang.GetText("Label_Progress") ?? "PROGRESSION";

        // LISTES FILTRÉES ET COMPTEURS
        public IEnumerable<BackupJob> ActiveJobs => Jobs.Where(j =>
            j.backupProgress.State == BackupState.Active ||
            j.backupProgress.State == BackupState.Paused ||
            j.backupProgress.State == BackupState.Inactive).ToList();

        public IEnumerable<BackupJob> FinishedJobs => Jobs.Where(j =>
            j.backupProgress.State == BackupState.Ended ||
            j.backupProgress.State == BackupState.Failed ||
            j.backupProgress.State == BackupState.Stopped).ToList();

        public int ActiveCount => ActiveJobs.Count();
        public int FinishedCount => FinishedJobs.Count();

        // COMMANDES
        public ICommand CreateCommand { get; }
        public IAsyncRelayCommand ExecuteAllCommand { get; }
        public ICommand EditJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public IAsyncRelayCommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }

        public DashboardViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _backupManager = BackupManager.Instance;

            LoadInitialJobs();

            CreateCommand = new RelayCommand(() => _navigation.CurrentPage = new JobEditorViewModel(_navigation));

            // Commande Tout Exécuter en PARALLÈLE
            ExecuteAllCommand = new AsyncRelayCommand(async () => {
                var targetJobs = Jobs.Where(j => j.backupProgress.State != BackupState.Ended).ToList();

                // On crée toutes les tâches de lancement
                var launchTasks = targetJobs.Select(job => ExecuteJobWithProtection(job));

                // On les lance toutes en même temps
                await Task.WhenAll(launchTasks);

            }, AsyncRelayCommandOptions.AllowConcurrentExecutions);

            // Commande Play individuelle
            PlayCommand = new AsyncRelayCommand<BackupJob>(async j => {
                if (j == null) return;
                await ExecuteJobWithProtection(j);
            }, AsyncRelayCommandOptions.AllowConcurrentExecutions);

            EditJobCommand = new RelayCommand<BackupJob>(j => {
                if (j == null) return;
                int idx = _backupManager.ListJobs().FindIndex(x => x.name == j.name);
                _navigation.CurrentPage = new JobEditorViewModel(_navigation, j, idx);
            });

            DeleteJobCommand = new RelayCommand<BackupJob>(j => {
                int idx = _backupManager.ListJobs().FindIndex(x => x.name == j?.name);
                if (idx != -1) _backupManager.DeleteJob(idx);
                Jobs.Remove(j);
                UpdateSections();
            });

            PauseCommand = new RelayCommand<BackupJob>(j => {
                int idx = _backupManager.ListJobs().FindIndex(x => x.name == j?.name);
                if (idx != -1) _backupManager.PauseJob(idx);
            });

            StopCommand = new RelayCommand<BackupJob>(j => {
                int idx = _backupManager.ListJobs().FindIndex(x => x.name == j?.name);
                if (idx != -1) _backupManager.StopJob(idx);
            });

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _refreshTimer.Tick += (s, e) => RefreshUI();
            _refreshTimer.Start();
        }

        // Méthode pour protéger l'état du job lors du lancement
        private async Task ExecuteJobWithProtection(BackupJob job)
        {
            int idx = _backupManager.ListJobs().FindIndex(x => x.name == job.name);
            if (idx == -1) return;

            // Si le logiciel métier n'est pas lancé, on force l'état visuel et on le protège
            if (!_backupManager.IsForbiddenSoftwareRunning())
            {
                lock (_protectedJobs)
                {
                    _protectedJobs[job.name] = DateTime.Now.AddSeconds(3);
                }
                job.backupProgress.State = BackupState.Active;
                UpdateSections();
            }

            // Lancement effectif dans le Core
            if (job.backupProgress.State == BackupState.Paused)
                await Task.Run(() => _backupManager.ResumeJob(idx));
            else
                await Task.Run(() => _backupManager.ExecuteJob(idx));
        }

        private void RefreshUI()
        {
            var fresh = _backupManager.ListJobs();
            if (fresh == null) return;

            bool needsUpdate = false;
            DateTime now = DateTime.Now;

            foreach (var f in fresh)
            {
                var ex = Jobs.FirstOrDefault(j => j.name == f.name);
                if (ex != null)
                {
                    bool isProtected = false;
                    lock (_protectedJobs)
                    {
                        if (_protectedJobs.ContainsKey(ex.name))
                        {
                            if (now < _protectedJobs[ex.name]) isProtected = true;
                            else _protectedJobs.Remove(ex.name);
                        }
                    }

                    // On ne synchronise l'état que si le job n'est pas protégé
                    if (!isProtected && ex.backupProgress.State != f.backupProgress.State)
                    {
                        ex.backupProgress.State = f.backupProgress.State;
                        needsUpdate = true;
                    }

                    // Mise à jour des données (progression, date)
                    ex.backupProgress.Progress = f.backupProgress.Progress;
                    ex.backupProgress.DateTime = f.backupProgress.DateTime;
                }
            }

            if (needsUpdate) UpdateSections();

            // Rafraîchissement des textes (Langue)
            OnPropertyChanged(nameof(TitleText));
            OnPropertyChanged(nameof(ActiveSectionText));
            OnPropertyChanged(nameof(HistorySectionText));
        }

        private void LoadInitialJobs()
        {
            var list = _backupManager.ListJobs() ?? new List<BackupJob>();
            Jobs = new ObservableCollection<BackupJob>(list);
        }

        private void UpdateSections()
        {
            OnPropertyChanged(nameof(ActiveJobs));
            OnPropertyChanged(nameof(FinishedJobs));
            OnPropertyChanged(nameof(ActiveCount));
            OnPropertyChanged(nameof(FinishedCount));
        }
    }
}