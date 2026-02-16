using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.GUI.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
