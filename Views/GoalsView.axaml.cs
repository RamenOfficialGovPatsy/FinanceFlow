using Avalonia.Controls;
using Avalonia.Interactivity;
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

        private async void AnalyticsButton_Click(object? sender, RoutedEventArgs e)
        {
            var analyticsWindow = new AnalyticsWindow();

            // ViewModel подключим позже (на 4 этапе)
            // analyticsWindow.DataContext = new AnalyticsViewModel(); 

            if (VisualRoot is Window parentWindow)
            {
                await analyticsWindow.ShowDialog(parentWindow);
            }
        }

        // Обработчик кнопки "Новая цель"
        private void AddGoalButton_Click(object? sender, RoutedEventArgs e)
        {
            var addGaolWindow = new AddGoalWindow();
            addGaolWindow.DataContext = new AddEditGoalViewModel();
            addGaolWindow.Show();
        }

        // Обработчики контекстного меню
        private void EditGoalMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var editGoalWindow = new EditGoalWindow();
            editGoalWindow.DataContext = new AddEditGoalViewModel(isEditMode: true);
            editGoalWindow.Show();
        }

        private async void AddDepositMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var depositWindow = new DepositWindow();

            if (VisualRoot is Window parentWindow)
            {
                await depositWindow.ShowDialog(parentWindow);
            }
        }

        private void ShowHistoryMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            // Позже добавлю историю
        }

        private void DeleteGoalMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            // Позже добавлю подтверждение удаления
        }
    }
}