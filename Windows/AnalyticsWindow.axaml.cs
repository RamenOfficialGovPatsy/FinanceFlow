using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FinanceFlow.Windows
{
    public partial class AnalyticsWindow : Window
    {
        public AnalyticsWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}