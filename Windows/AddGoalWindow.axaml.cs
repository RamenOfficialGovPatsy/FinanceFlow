using Avalonia;
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
                if (DataContext is AddEditGoalViewModel vm)
                {
                    vm.RequestClose += () => this.Close();
                }
            };
            //  DataContext = new AddEditGoalViewModel(); // Автоматически режим создания
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Обработчик кнопки "Отмена"
        /*
        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }
        */

        // Обработчик кнопки "Сохранить" 
        /*
        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }
        */
    }
}