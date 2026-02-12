using System.Windows.Input;
using EasySave.Core.Managers;
using RelayCommand = EasySave.GUI.Commands.RelayCommand;

namespace EasySave.GUI.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _navigation;
        private readonly LanguageManager _lang;

        // Propriétés pour l'interface (Bindings)

        
        public string StartButtonText => _lang.GetText("Btn_Start");
        public string FrenchLabel => _lang.GetText("lang_FR");
        public string EnglishLabel => _lang.GetText("lang_EN");

    
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
                    RefreshTexts(); // Force la mise à jour des labels
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
                    RefreshTexts(); // Force la mise à jour des labels
                }
            }
        }

        public HomeViewModel(MainWindowViewModel navigation)
        {
            _navigation = navigation;
            _lang = LanguageManager.Instance;

            // On initialise la commande
            StartCommand = new RelayCommand(() => _navigation.NavigateToDashboard());

            // Langue par défaut
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
