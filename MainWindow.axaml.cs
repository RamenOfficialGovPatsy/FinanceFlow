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

            // Устанавливаем Content здесь
            Content = new GoalsView();

            // DataContext
            //  DataContext = new MainWindowViewModel(null);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}