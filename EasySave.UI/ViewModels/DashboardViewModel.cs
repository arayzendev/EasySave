using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.GUI.Commands;
using RelayCommand = EasySave.GUI.Commands.RelayCommand;

namespace EasySave.GUI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly BackupManager _backupManager;
        private readonly LanguageManager _lang = LanguageManager.Instance;
        private readonly DispatcherTimer _refreshTimer;

        private ObservableCollection<BackupJob> _jobs;
        public ObservableCollection<BackupJob> Jobs
        {
            get => _jobs;
            set { _jobs = value; OnPropertyChanged(); }
        }

        // --- TRADUCTION DYNAMIQUE ---
        // On utilise => pour forcer la lecture du manager à chaque demande
        public string TitleText => _lang.GetText("Menu_Titre") ?? "Dashboard";
        public string CreateBtnText => _lang.GetText("Menu_Create") ?? "Nouveau";
        public string ExecuteAllBtnText => _lang.GetText("Menu_ExecuteAll") ?? "Tout Exécuter";
        public string EditBtnText => _lang.GetText("Menu_Modify") ?? "Modifier";
        public string DeleteBtnText => _lang.GetText("Menu_Delete") ?? "Supprimer";

        public ICommand CreateCommand { get; }
        public ICommand ExecuteAllCommand { get; }
        public ICommand EditJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }

        public DashboardViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _backupManager = BackupManager.Instance;
            Jobs = new ObservableCollection<BackupJob>(_backupManager.ListJobs());

            CreateCommand = new RelayCommand(() => _navigation.CurrentPage = new JobEditorViewModel(_navigation));

            ExecuteAllCommand = new RelayCommand(async () => {
                var jobsList = Jobs.ToList();
                await Task.Run(() => {
                    for (int i = 0; i < jobsList.Count; i++)
                    {
                        _backupManager.ExecuteJob(i);
                    }
                });
            });

            EditJobCommand = new RelayCommand<BackupJob>((job) => {
                _navigation.CurrentPage = new JobEditorViewModel(_navigation, job, Jobs.IndexOf(job));
            });

            DeleteJobCommand = new RelayCommand<BackupJob>((job) => {
                int index = Jobs.IndexOf(job);
                _backupManager.DeleteJob(index);
                Jobs.Remove(job);
            });

            PlayCommand = new RelayCommand<BackupJob>(async (job) => {
                int index = Jobs.IndexOf(job);
                if (index != -1)
                {
                    if (job.backupProgress.State == BackupState.Paused)
                    {
                        await Task.Run(() => _backupManager.ResumeJob(index));
                    }
                    else if (job.backupProgress.State != BackupState.Active)
                    {
                        await Task.Run(() => _backupManager.ExecuteJob(index));
                    }
                }
            });

            PauseCommand = new RelayCommand<BackupJob>((job) => {
                int index = Jobs.IndexOf(job);
                if (index != -1 && job.backupProgress.State == BackupState.Active)
                {
                    _backupManager.PauseJob(index);
                }
            });

            StopCommand = new RelayCommand<BackupJob>((job) => {
                int index = Jobs.IndexOf(job);
                if (index != -1 && job.backupProgress.State == BackupState.Active)
                {
                    _backupManager.StopJob(index);
                }
            });

            // --- REFRESH TIMER ---
            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _refreshTimer.Tick += (s, e) =>
            {
                // Mise à jour des propriétés des jobs existants en place
                var freshJobs = _backupManager.ListJobs();
                for (int i = 0; i < freshJobs.Count && i < Jobs.Count; i++)
                {
                    var fresh = freshJobs[i];
                    var job = Jobs[i];
                    
                    job.name = fresh.name;
                    job.sourcePath = fresh.sourcePath;
                    job.targetPath = fresh.targetPath;
                    job.strategyType = fresh.strategyType;
                    
                    // Mise à jour des propriétés de backupProgress en place (pas de remplacement d'objet)
                    job.backupProgress.State = fresh.backupProgress.State;
                    job.backupProgress.Progress = fresh.backupProgress.Progress;
                    job.backupProgress.TotalFiles = fresh.backupProgress.TotalFiles;
                    job.backupProgress.TotalSize = fresh.backupProgress.TotalSize;
                    job.backupProgress.FileSize = fresh.backupProgress.FileSize;
                    job.backupProgress.TransferTime = fresh.backupProgress.TransferTime;
                    job.backupProgress.RemainingFiles = fresh.backupProgress.RemainingFiles;
                    job.backupProgress.RemainingSize = fresh.backupProgress.RemainingSize;
                    job.backupProgress.DateTime = fresh.backupProgress.DateTime;
                }

                // FORCE LE RAFRAÎCHISSEMENT DES TEXTES (Langue)
                OnPropertyChanged(nameof(TitleText));
                OnPropertyChanged(nameof(CreateBtnText));
                OnPropertyChanged(nameof(ExecuteAllBtnText));
                OnPropertyChanged(nameof(EditBtnText));
                OnPropertyChanged(nameof(DeleteBtnText));
            };
            _refreshTimer.Start();
        }
    }
}