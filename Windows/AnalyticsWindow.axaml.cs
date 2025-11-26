using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FinanceFlow.ViewModels;

namespace FinanceFlow.Windows
{
    public partial class AnalyticsWindow : Window
    {
        public AnalyticsWindow()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is AnalyticsViewModel vm)
                {
                    vm.RequestClose += () => this.Close();
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}