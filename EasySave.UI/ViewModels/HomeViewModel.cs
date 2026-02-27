using System;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using RelayCommand = EasySave.GUI.Commands.RelayCommand;
using EasyLog.Models;

namespace EasySave.GUI.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        // Champs privés 
        private readonly MainWindowViewModel _navigation;
        private readonly LanguageManager _lang;
        private readonly BackupManager _backupManager;

        // Propriétés pour l'interface (Bindings)

        private bool _isInputInvalid;
        private string _priorityExtensions;
        private string _forbiddenSoftware;
        private bool _isLocalLogEnabled;
        private bool _isDockerLogEnabled;
        private bool _isJsonSelected;
        private bool _isXmlSelected;

        // Propriétés publiques avec OnPropertyChanged 
        public bool IsInputInvalid
        {
            get
            {
                return this._isInputInvalid;
            }
            set
            {
                this._isInputInvalid = value;
                this.OnPropertyChanged(nameof(IsInputInvalid));
            }
        }

        public string PriorityExtensions
        {
            get
            {
                return this._priorityExtensions;
            }
            set
            {
                this._priorityExtensions = value;
                this.OnPropertyChanged(nameof(PriorityExtensions));
                this.RefreshCommands();
            }
        }
        //logiciel interdit
        public string ForbiddenSoftware
        {
            get
            {
                return this._forbiddenSoftware;
            }
            set
            {
                this._forbiddenSoftware = value;
                this.OnPropertyChanged(nameof(ForbiddenSoftware));
                this.RefreshCommands();
            }
        }

        //  Commandes
        public ICommand StartCommand { get; }
        public ICommand SaveSoftwareCommand { get; }
        public ICommand SavePriorityCommand { get; }

        // Constructeur
        public HomeViewModel(MainWindowViewModel navigation)
        {
            this._navigation = navigation;
            this._lang = LanguageManager.Instance;
            this._backupManager = BackupManager.Instance;

            // Initialisation des commandes avec délégués explicites
            this.StartCommand = new RelayCommand(
                execute: () =>
                {
                    this._navigation.CurrentPage = new DashboardViewModel(this._navigation);
                }
            );

            this.SaveSoftwareCommand = new RelayCommand(
                execute: async () =>
                {
                    this._backupManager.SetForbiddenSoftware(this.ForbiddenSoftware);
                },
                canExecute: () =>
                {
                    return this.IsSoftwareValid();
                }
            );

            this.SavePriorityCommand = new RelayCommand(
                execute: async () =>
                {
                    List<string> list = this.PriorityExtensions
                        .Split(';')
                        .Select(e => e.Trim().ToLower())
                        .Where(e => !string.IsNullOrEmpty(e))
                        .ToList();
                    this._backupManager.UpdatePriorityExtensions(list);
                },
                canExecute: () =>
                {
                    return this.IsExtensionsValid();
                }
            );

            // Initialisation des données par défaut
            this._forbiddenSoftware = string.Empty;
            List<string> currentExtensions = this._backupManager.GetPriorityExtensions() ?? new List<string>();
            this._priorityExtensions = string.Join("; ", currentExtensions);

            // Initialisation du mode de log depuis la config
            var currentLogMode = this._backupManager.GetLogMode();
            this._isLocalLogEnabled = currentLogMode == LogMode.Local || currentLogMode == LogMode.Composite;
            this._isDockerLogEnabled = currentLogMode == LogMode.Docker || currentLogMode == LogMode.Composite;

            // Initialisation du format de log depuis la config
            var currentLogType = this._backupManager.GetLogType();
            this._isJsonSelected = currentLogType == LogType.JSON;
            this._isXmlSelected = currentLogType == LogType.XML;

            // Définition de la langue initiale
            this._lang.SetLanguage("FR");
        }

        // Méthodes de validation
        private bool IsSoftwareValid()
        {
            if (string.IsNullOrWhiteSpace(this.ForbiddenSoftware))
            {
                return true; // Champ optionnel
            }
            return this.ForbiddenSoftware.Trim().Length >= 2;
        }

        private bool IsExtensionsValid()
        {
            if (string.IsNullOrWhiteSpace(this.PriorityExtensions))
            {
                return false;
            }

            // Regex pour format exigé : .ext; .ext2
            string pattern = @"^\.[a-zA-Z0-9]+(;\s?\.[a-zA-Z0-9]+)*$";
            return Regex.IsMatch(this.PriorityExtensions.Trim(), pattern);
        }

        // Méthodes de rafraîchissement
        private void RefreshCommands()
        {
            if (this.SaveSoftwareCommand is RelayCommand cmd2)
            {
                cmd2.RaiseCanExecuteChanged();
            }
            if (this.SavePriorityCommand is RelayCommand cmd3)
            {
                cmd3.RaiseCanExecuteChanged();
            }
        }

        // Propriétés de Langue et format Logs 
        public bool IsFrenchSelected
        {
            get
            {
                return this._lang.CurrentLanguage == "FR";
            }
            set
            {
                if (value)
                {
                    this._lang.SetLanguage("FR");
                    this.RefreshTexts();
                }
            }
        }

        public bool IsEnglishSelected
        {
            get
            {
                return this._lang.CurrentLanguage == "EN";
            }
            set
            {
                if (value)
                {
                    this._lang.SetLanguage("EN");
                    this.RefreshTexts();
                }
            }
        }

        public bool IsJsonSelected
        {
            get
            {
                return this._isJsonSelected;
            }
            set
            {
                this._isJsonSelected = value;
                this.OnPropertyChanged(nameof(IsJsonSelected));
                if (value)
                {
                    this._backupManager.SetLog("JSON");
                }
            }
        }

        public bool IsXmlSelected
        {
            get
            {
                return this._isXmlSelected;
            }
            set
            {
                this._isXmlSelected = value;
                this.OnPropertyChanged(nameof(IsXmlSelected));
                if (value)
                {
                    this._backupManager.SetLog("XML");
                }
            }
        }

        public bool IsLocalLogEnabled
        {
            get
            {
                return this._isLocalLogEnabled;
            }
            set
            {
                this._isLocalLogEnabled = value;
                this.OnPropertyChanged(nameof(IsLocalLogEnabled));
                this.UpdateLogMode();
            }
        }

        public bool IsDockerLogEnabled
        {
            get
            {
                return this._isDockerLogEnabled;
            }
            set
            {
                this._isDockerLogEnabled = value;
                this.OnPropertyChanged(nameof(IsDockerLogEnabled));
                this.UpdateLogMode();
            }
        }

        private void UpdateLogMode()
        {
            if (this._isLocalLogEnabled && this._isDockerLogEnabled)
            {
                this._backupManager.SetLogMode("all");
            }
            else if (this._isDockerLogEnabled)
            {
                this._backupManager.SetLogMode("docker");
            }
            else
            {
                this._backupManager.SetLogMode("local");
            }
        }

        // Accesseurs de Textes traduits
        public string StartButtonText { get { return this._lang.GetText("Btn_Start"); } }
        public string FrenchLabel { get { return this._lang.GetText("lang_FR"); } }
        public string EnglishLabel { get { return this._lang.GetText("lang_EN"); } }
        public string VersionLabel { get { return this._lang.GetText("Home_Version"); } }
        public string GeneralSettingsTitle { get { return this._lang.GetText("Home_GeneralTitle"); } }
        public string LanguageSelectionLabel { get { return this._lang.GetText("Home_LangLabel"); } }
        public string LogFormatLabel { get { return this._lang.GetText("Home_LogLabel"); } }
        public string LogModeLabel { get { return this._lang.GetText("Home_LogModeLabel"); } }
        public string SecuritySettingsTitle { get { return this._lang.GetText("Home_SecurityTitle"); } }
        public string BusinessSoftwareLabel { get { return this._lang.GetText("Home_SoftwareLabel"); } }
        public string PriorityExtensionsLabel { get { return this._lang.GetText("Home_ExtensionsLabel"); } }
        public string FooterLabel { get { return this._lang.GetText("Home_Footer"); } }
        public string SoftwareWatermark { get { return "Ex: Calculator, chrome.exe"; } }
        public string ExtensionsWatermark { get { return "Ex: .rar; .pdf; .xml"; } }

        // Méthodes asynchrones et utilitaires
        private async Task TriggerErrorAnimation()
        {
            this.IsInputInvalid = true;
            await Task.Delay(500);
            this.IsInputInvalid = false;
        }

        private void RefreshTexts()
        {
            //Rafraîchit l'intégralité des bindings de la vue
            this.OnPropertyChanged(string.Empty);
        }
    }
}