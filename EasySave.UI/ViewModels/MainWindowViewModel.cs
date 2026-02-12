using EasySave.GUI.ViewModels;

namespace EasySave.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentPage;

        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            // On démarre sur la page d'accueil
            _currentPage = new HomeViewModel(this);
        }

        public void NavigateToDashboard()
        {
            // Logique de navigation future
        }
    }
}