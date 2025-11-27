using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FinanceFlow.ViewModels;

namespace FinanceFlow.Windows
{
    public partial class EditGoalWindow : Window
    {
        public EditGoalWindow()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                // 1. Подписка на закрытие
                if (DataContext is AddEditGoalViewModel vm)
                {
                    vm.RequestClose += () => this.Close();
                }

                // 2. Подписка на уведомления/ошибки
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