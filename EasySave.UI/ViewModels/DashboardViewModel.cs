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
        private static string _lastModifiedJobName = null;

        private ObservableCollection<BackupJob> _jobs;
        public ObservableCollection<BackupJob> Jobs
        {
            get => _jobs;
            set { _jobs = value; OnPropertyChanged(); UpdateSections(); }
        }

        // Listes filtrées pour le XAML
        public IEnumerable<BackupJob> ActiveJobs => Jobs.Where(j =>
            j.backupProgress.State == BackupState.Active || j.backupProgress.State == BackupState.Paused || j.backupProgress.State == BackupState.Inactive);

        public IEnumerable<BackupJob> FinishedJobs => Jobs.Where(j =>
            j.backupProgress.State == BackupState.Ended || j.backupProgress.State == BackupState.Failed || j.backupProgress.State == BackupState.Stopped);

        // Propriétés de traduction et compteurs
        public int ActiveCount => ActiveJobs.Count();
        public int FinishedCount => FinishedJobs.Count();
        public string TitleText => _lang.GetText("Menu_Titre") ?? "DASHBOARD";
        public string ActiveSectionText => _lang.GetText("Label_ActiveMissions") ?? "MISSIONS ACTIVES";
        public string HistorySectionText => _lang.GetText("Label_History") ?? "HISTORIQUE";
        public string ExecuteAllBtnText => _lang.GetText("Menu_ExecuteAll") ?? "EXECUTE ALL";
        public string ProgressLabelText => _lang.GetText("Label_Progress") ?? "PROGRESS";

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
            LoadAndSortJobs();

            CreateCommand = new RelayCommand(() => _navigation.CurrentPage = new JobEditorViewModel(_navigation));

            ExecuteAllCommand = new AsyncRelayCommand(async () => {
                await Task.Run(() => {
                    foreach (var job in Jobs.ToList())
                    {
                        if (job.backupProgress.State != BackupState.Ended)
                        {
                            int idx = _backupManager.ListJobs().FindIndex(j => j.name == job.name);
                            if (idx != -1) _backupManager.ExecuteJob(idx);
                        }
                    }
                });
            });

            EditJobCommand = new RelayCommand<BackupJob>(j => {
                if (j == null) return;
                _lastModifiedJobName = j.name;
                _navigation.CurrentPage = new JobEditorViewModel(_navigation, j, _backupManager.ListJobs().FindIndex(x => x.name == j.name));
            });

            DeleteJobCommand = new RelayCommand<BackupJob>(j => {
                int idx = _backupManager.ListJobs().FindIndex(x => x.name == j?.name);
                if (idx != -1) _backupManager.DeleteJob(idx);
                Jobs.Remove(j);
                UpdateSections();
            });

            PlayCommand = new AsyncRelayCommand<BackupJob>(async j => {
                int idx = _backupManager.ListJobs().FindIndex(x => x.name == j?.name);
                if (idx == -1) return;
                if (j.backupProgress.State == BackupState.Paused) await Task.Run(() => _backupManager.ResumeJob(idx));
                else await Task.Run(() => _backupManager.ExecuteJob(idx));
            });

            PauseCommand = new RelayCommand<BackupJob>(j => _backupManager.PauseJob(_backupManager.ListJobs().FindIndex(x => x.name == j?.name)));
            StopCommand = new RelayCommand<BackupJob>(j => _backupManager.StopJob(_backupManager.ListJobs().IndexOf(j)));

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _refreshTimer.Tick += (s, e) => RefreshUI();
            _refreshTimer.Start();
        }

        public static void SetLastModified(string name) => _lastModifiedJobName = name;

        private void LoadAndSortJobs()
        {
            var list = _backupManager.ListJobs() ?? new List<BackupJob>();
            Jobs = new ObservableCollection<BackupJob>(list.OrderBy(GetJobPriority));
        }

        private int GetJobPriority(BackupJob j)
        {
            if (j.backupProgress.State == BackupState.Ended || j.backupProgress.State == BackupState.Failed || j.backupProgress.State == BackupState.Stopped) return 3;
            if (j.name == _lastModifiedJobName) return 0;
            if (j.backupProgress.State == BackupState.Active || j.backupProgress.State == BackupState.Paused) return 1;
            return 2;
        }

        private void RefreshUI()
        {
            var fresh = _backupManager.ListJobs();
            if (fresh == null) return;
            bool needsSort = false;
            foreach (var f in fresh)
            {
                var ex = Jobs.FirstOrDefault(j => j.name == f.name);
                if (ex != null)
                {
                    if (ex.backupProgress.State != f.backupProgress.State) { ex.backupProgress.State = f.backupProgress.State; needsSort = true; }
                    ex.backupProgress.Progress = f.backupProgress.Progress;
                    ex.backupProgress.DateTime = f.backupProgress.DateTime;
                }
                else { LoadAndSortJobs(); return; }
            }
            if (needsSort) Dispatcher.UIThread.Post(() => {
                var sorted = Jobs.OrderBy(GetJobPriority).ToList();
                for (int i = 0; i < sorted.Count; i++)
                {
                    int old = Jobs.IndexOf(sorted[i]);
                    if (old != i && old != -1) Jobs.Move(old, i);
                }
                UpdateSections();
            }, DispatcherPriority.Background);
            OnPropertyChanged(nameof(TitleText)); OnPropertyChanged(nameof(ActiveSectionText)); OnPropertyChanged(nameof(HistorySectionText));
        }

        private void UpdateSections()
        {
            OnPropertyChanged(nameof(ActiveJobs)); OnPropertyChanged(nameof(FinishedJobs));
            OnPropertyChanged(nameof(ActiveCount)); OnPropertyChanged(nameof(FinishedCount));
        }
    }
}