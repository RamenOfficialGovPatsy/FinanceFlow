using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FinanceFlow.ViewModels;
using FinanceFlow.Windows;

namespace FinanceFlow.Views
{
    public partial class GoalsView : UserControl
    {
        public GoalsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Обработчик кнопки "Новая цель"
        private void AddGoalButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var addGaolWindow = new AddGoalWindow();
            addGaolWindow.DataContext = new AddEditGoalViewModel();
            addGaolWindow.Show();
        }

        // Обработчики контекстного меню
        private void EditGoalMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var editGoalWindow = new EditGoalWindow();
            editGoalWindow.DataContext = new AddEditGoalViewModel(isEditMode: true);
            editGoalWindow.Show();
        }

        private void AddDepositMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var depositWindow = new DepositWindow();
            depositWindow.ShowDialog(VisualRoot as Window);
        }

        private void ShowHistoryMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Позже добавлю историю
        }

        private void DeleteGoalMenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Позже добавлю подтверждение удаления
        }
    }
}