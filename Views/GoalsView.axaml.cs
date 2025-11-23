using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FinanceFlow.Services.Interfaces;
using FinanceFlow.ViewModels;
using FinanceFlow.Windows;
using Microsoft.Extensions.DependencyInjection; // Для GetRequiredService

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
            if (VisualRoot is Window parentWindow)
            {
                await analyticsWindow.ShowDialog(parentWindow);
            }
        }

        // Вспомогательный метод для получения сервиса
        private IGoalService GetGoalService()
        {
            var app = (App)Application.Current!;
            return app.Services!.GetRequiredService<IGoalService>();
        }

        private async void AddGoalButton_Click(object? sender, RoutedEventArgs e)
        {
            var addGoalWindow = new AddGoalWindow();

            // ВНЕДРЯЕМ СЕРВИС В VIEWMODEL
            var goalService = GetGoalService();
            addGoalWindow.DataContext = new AddEditGoalViewModel(goalService);

            if (VisualRoot is Window parentWindow)
            {
                await addGoalWindow.ShowDialog(parentWindow);

                if (DataContext is MainWindowViewModel vm)
                {
                    await vm.LoadGoalsAsync();
                }
            }
        }

        private async void EditGoalMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is GoalViewModel selectedGoal)
            {
                var editGoalWindow = new EditGoalWindow();

                // ВНЕДРЯЕМ СЕРВИС И ЦЕЛЬ
                var goalService = GetGoalService();
                editGoalWindow.DataContext = new AddEditGoalViewModel(goalService, selectedGoal.GetGoalModel());

                if (VisualRoot is Window parentWindow)
                {
                    await editGoalWindow.ShowDialog(parentWindow);

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
                depositWindow.DataContext = new DepositViewModel(selectedGoal.GetGoalModel());

                if (VisualRoot is Window parentWindow)
                {
                    await depositWindow.ShowDialog(parentWindow);

                    if (DataContext is MainWindowViewModel vm)
                    {
                        await vm.LoadGoalsAsync();
                    }
                }
            }
        }
    }
}