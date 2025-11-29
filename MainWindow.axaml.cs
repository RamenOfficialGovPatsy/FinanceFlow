using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FinanceFlow.Views;

namespace FinanceFlow
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Content = new GoalsView();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}