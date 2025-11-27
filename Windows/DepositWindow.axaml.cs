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

            // Подписываемся на события
            this.DataContextChanged += (s, e) =>
            {
                // 1. Закрытие окна (уже было, но проверяем)
                if (DataContext is DepositViewModel vm)
                {
                    vm.RequestClose += () => this.Close();
                }

                // 2. ВСТАВИТЬ ЭТОТ БЛОК: Подписка на ошибки/уведомления
                if (DataContext is ViewModelBase baseVm)
                {
                    baseVm.RequestNotification += (title, message) =>
                    {
                        var msgBox = new MessageBoxWindow(title, message);
                        msgBox.ShowDialog(this);
                    };
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}