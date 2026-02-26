using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.GUI.Commands;
using EasySave.Core.Models;

namespace EasySave.GUI.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly LanguageManager _lang = LanguageManager.Instance;
        private readonly MainWindowViewModel _navigation;

      
        public string Title => _lang.GetText("Menu_Titre");
        public string CreateText => CleanText(_lang.GetText("Menu_Create"));
        public string ListText => CleanText(_lang.GetText("Menu_List"));
        public string ModifyText => CleanText(_lang.GetText("Menu_Modify"));
        public string DeleteText => CleanText(_lang.GetText("Menu_Delete"));
        public string ExecuteText => CleanText(_lang.GetText("Menu_Execute"));
        public string QuitText => CleanText(_lang.GetText("Menu_Quit"));

        public ICommand CreateCommand { get; }
        public ICommand ListCommand { get; }
        public ICommand ModifyCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExecuteCommand { get; }
        public ICommand QuitCommand { get; }

        public MainMenuViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;


            //Navigation vers Createjob

            CreateCommand = new RelayCommand(() =>
            {
                _navigation.CurrentPage = new JobEditorViewModel(_navigation);
            });

            //Navigation vers ListJob
            ListCommand = new RelayCommand(() =>
            {
                _navigation.CurrentPage = new ListJobsViewModel(_navigation);
            });

            ModifyCommand = new RelayCommand(() =>
            {
                _navigation.CurrentPage = new ListJobsViewModel(_navigation);
            });
            DeleteCommand = new RelayCommand(() =>
            {
                _navigation.CurrentPage = new ListJobsViewModel(_navigation);
            });

            //Navigation vers ExecuteJob
            ExecuteCommand = new RelayCommand(() =>
            {
                _navigation.CurrentPage = new ExecuteJobsViewModel(_navigation);
            });

            // Retour accueil
            QuitCommand = new RelayCommand(() => _navigation.CurrentPage = new HomeViewModel(_navigation));
        }

        // méthode pour enlever "1. ", "2. "
        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // On cherche la position du point
            int index = text.IndexOf('.');

            // Si on trouve un point, on prend tout ce qui est après le point
            if (index != -1 && text.Length > index + 1)
            {
                return text.Substring(index + 1).Trim();
            }

            return text; // Retourne le texte normal si pas de point trouvé
        }
    }
}