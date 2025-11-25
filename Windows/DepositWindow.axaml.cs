using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FinanceFlow.ViewModels;

namespace FinanceFlow.Windows
{
    public partial class DepositWindow : Window
    {
        public DepositWindow()
        {
            InitializeComponent();

            // Подписываемся на событие закрытия
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is DepositViewModel vm)
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