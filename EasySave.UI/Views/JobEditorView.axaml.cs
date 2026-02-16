
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.GUI.Views
{
    public partial class JobEditorView : UserControl
    {
        public JobEditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
