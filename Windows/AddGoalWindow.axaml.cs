using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FinanceFlow.ViewModels;

namespace FinanceFlow.Windows
{
    public partial class AddGoalWindow : Window
    {
        public AddGoalWindow()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                // 1. Подписка на закрытие (специфично для AddEditGoalViewModel)
                if (DataContext is AddEditGoalViewModel vm)
                {
                    vm.RequestClose += () => this.Close();
                }

                // 2. Подписка на уведомления/ошибки (от ViewModelBase)
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