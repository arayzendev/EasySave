using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using EasySave.GUI.Commands;
using EasySave.Core.Managers;
using EasySave.Core.Models;

namespace EasySave.GUI.ViewModels
{
    public class ExecuteJobsViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly BackupManager _backupManager;
        private bool _isExecuting;

        // Cette propriété est indispensable pour corriger l'erreur "S"
        public ObservableCollection<BackupJob> Jobs { get; set; }
        public ObservableCollection<BackupJob> SelectedJobs { get; } = new ObservableCollection<BackupJob>();

        public ICommand BackCommand { get; }
        public ICommand ExecuteAllCommand { get; }

        // Constructeur à 2 arguments pour corriger l'erreur CS1729
        public ExecuteJobsViewModel(MainWindowViewModel navigation, ObservableCollection<BackupJob> jobs)
        {
            _navigation = navigation;
            _backupManager = new BackupManager();
            Jobs = jobs;

            BackCommand = new RelayCommand(() => _navigation.CurrentPage = new MainMenuViewModel(_navigation));
            ExecuteAllCommand = new RelayCommand(async () => await RunBackups(Jobs.ToList()));
        }

        public ExecuteJobsViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
        }

        private async Task RunBackups(List<BackupJob> list)
        {
            if (_isExecuting) return;
            _isExecuting = true;

            foreach (var job in list)
            {
                int index = _backupManager.ListJobs().FindIndex(j => j.name == job.name);
                if (index != -1) await Task.Run(() => _backupManager.ExecuteJob(index));
            }
            _isExecuting = false;
        }
    }
}