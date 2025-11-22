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

            // ВАЖНО: Здесь мы потом подключим сервис аналитики
            // analyticsWindow.DataContext = new AnalyticsViewModel(...);

            if (VisualRoot is Window parentWindow)
            {
                await analyticsWindow.ShowDialog(parentWindow);
            }
        }

        private async void AddGoalButton_Click(object? sender, RoutedEventArgs e)
        {
            var addGoalWindow = new AddGoalWindow();
            // При закрытии окна обновляем список
            if (VisualRoot is Window parentWindow)
            {
                await addGoalWindow.ShowDialog(parentWindow);

                // Обновляем список после закрытия
                if (DataContext is MainWindowViewModel vm)
                {
                    await vm.LoadGoalsAsync();
                }
            }
        }

        private async void EditGoalMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            // Получаем цель, на которую кликнули (из Tag)
            if (sender is MenuItem menuItem && menuItem.Tag is GoalViewModel selectedGoal)
            {
                var editGoalWindow = new EditGoalWindow();
                // Передаем данные цели во ViewModel окна редактирования
                // Примечание: нам нужно будет передать реальную модель Goal, а не VM,
                // поэтому вызываем метод GetGoalModel()
                editGoalWindow.DataContext = new AddEditGoalViewModel(isEditMode: true, goalToEdit: selectedGoal.GetGoalModel());

                if (VisualRoot is Window parentWindow)
                {
                    await editGoalWindow.ShowDialog(parentWindow);

                    // Обновляем список
                    if (DataContext is MainWindowViewModel vm)
                    {
                        await vm.LoadGoalsAsync();
                    }
                }
            }
        }

        private async void AddDepositMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is GoalViewModel selectedGoal)
            {
                var depositWindow = new DepositWindow();
                // Передаем цель в ViewModel пополнения
                depositWindow.DataContext = new DepositViewModel(selectedGoal.GetGoalModel());

                if (VisualRoot is Window parentWindow)
                {
                    await depositWindow.ShowDialog(parentWindow);

                    // Обновляем список
                    if (DataContext is MainWindowViewModel vm)
                    {
                        await vm.LoadGoalsAsync();
                    }
                }
            }
        }
    }
}