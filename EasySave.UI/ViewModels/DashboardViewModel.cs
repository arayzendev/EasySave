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
        public ICommand PlayPauseCommand { get; }

        public DashboardViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _backupManager = new BackupManager();
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

            PlayPauseCommand = new RelayCommand<BackupJob>(async (job) => {
                int index = Jobs.IndexOf(job);
                if (index != -1)
                {
                    await Task.Run(() => _backupManager.ExecuteJob(index));
                }
            });

            // --- REFRESH TIMER ---
            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _refreshTimer.Tick += (s, e) =>
            {
                // Force le rafraîchissement de la liste (Barre de progression et État)
                OnPropertyChanged(nameof(Jobs));

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