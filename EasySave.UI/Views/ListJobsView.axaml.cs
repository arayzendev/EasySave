using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.GUI.Views
{
    public partial class ListJobsView : UserControl
    {
        public ListJobsView()
        {
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}