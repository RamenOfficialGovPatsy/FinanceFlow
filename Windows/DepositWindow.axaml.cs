using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FinanceFlow.Windows
{
    public partial class DepositWindow : Window
    {
        public DepositWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}