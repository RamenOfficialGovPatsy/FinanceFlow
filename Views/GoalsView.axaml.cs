using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FinanceFlow.Views
{
    public partial class GoalsView : UserControl
    {
        public GoalsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}