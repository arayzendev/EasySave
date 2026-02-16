using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.GUI.Views
{
    public partial class MainMenuView : UserControl
    {
        public MainMenuView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}