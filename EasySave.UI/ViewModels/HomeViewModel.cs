using System.Windows.Input;
using EasySave.Core.Managers;
using RelayCommand = EasySave.GUI.Commands.RelayCommand;

namespace EasySave.GUI.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly LanguageManager _lang;
        private readonly BackupManager _backupManager;

        // Propriétés pour l'interface (Bindings)

        
        public string StartButtonText => _lang.GetText("Btn_Start");
        public string FrenchLabel => _lang.GetText("lang_FR");
        public string EnglishLabel => _lang.GetText("lang_EN");
        public string JsonLabel => "JSON";
        public string XmlLabel => "XML";

    
        public ICommand StartCommand { get; }

        private bool _isFrenchSelected = true;
        public bool IsFrenchSelected
        {
            get => _isFrenchSelected;
            set
            {
                if (_isFrenchSelected != value)
                {
                    _isFrenchSelected = value;
                    if (value) _lang.SetLanguage("FR");
                    OnPropertyChanged();
                    RefreshTexts();
                }
            }
        }

        private bool _isEnglishSelected;
        public bool IsEnglishSelected
        {
            get => _isEnglishSelected;
            set
            {
                if (_isEnglishSelected != value)
                {
                    _isEnglishSelected = value;
                    if (value) _lang.SetLanguage("EN");
                    OnPropertyChanged();
                    RefreshTexts();
                }
            }
        }

        private bool _isJsonSelected = true;
        public bool IsJsonSelected
        {
            get => _isJsonSelected;
            set
            {
                if (_isJsonSelected != value)
                {
                    _isJsonSelected = value;
                    if (value) _backupManager.SetLog("JSON");
                    OnPropertyChanged();
                }
            }
        }

        private bool _isXmlSelected;
        public bool IsXmlSelected
        {
            get => _isXmlSelected;
            set
            {
                if (_isXmlSelected != value)
                {
                    _isXmlSelected = value;
                    if (value) _backupManager.SetLog("XML");
                    OnPropertyChanged();
                }
            }
        }

        public HomeViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _lang = LanguageManager.Instance;
            _backupManager = new BackupManager();

            StartCommand = new RelayCommand(() =>
            {
                _navigation.CurrentPage = new DashboardViewModel(_navigation);
            });

            _lang.SetLanguage("FR");
        }

        
        private void RefreshTexts()
        {
            OnPropertyChanged(nameof(StartButtonText));
            OnPropertyChanged(nameof(FrenchLabel));
            OnPropertyChanged(nameof(EnglishLabel));
        }
    }
}
