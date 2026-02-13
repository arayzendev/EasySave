
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySave.Core.Models;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views
{
    public partial class ExecuteJobsView : UserControl
    {
        public ExecuteJobsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        // Synchronise la sélection de la ListBox avec la collection du ViewModel
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ExecuteJobsViewModel vm)
            {
                foreach (var item in e.AddedItems.Cast<BackupJob>())
                    if (!vm.SelectedJobs.Contains(item)) vm.SelectedJobs.Add(item);

                foreach (var item in e.RemovedItems.Cast<BackupJob>())
                    vm.SelectedJobs.Remove(item);
            }
        }
    }
}