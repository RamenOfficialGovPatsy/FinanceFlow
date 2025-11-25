using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FinanceFlow.Services.Interfaces;
using FinanceFlow.ViewModels;
using FinanceFlow.Windows;
using Microsoft.Extensions.DependencyInjection;

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

        private T GetService<T>() where T : notnull
        {
            var app = (App)Application.Current!;
            return app.Services!.GetRequiredService<T>();
        }

        private async void AnalyticsButton_Click(object? sender, RoutedEventArgs e)
        {
            var analyticsWindow = new AnalyticsWindow();
            if (VisualRoot is Window parentWindow)
            {
                await analyticsWindow.ShowDialog(parentWindow);
            }
        }

        private async void AddGoalButton_Click(object? sender, RoutedEventArgs e)
        {
            var addGoalWindow = new AddGoalWindow();
            var goalService = GetService<IGoalService>();
            addGoalWindow.DataContext = new AddEditGoalViewModel(goalService);

            if (VisualRoot is Window parentWindow)
            {
                await addGoalWindow.ShowDialog(parentWindow);
                // Обновляем после закрытия (для создания)
                if (DataContext is MainWindowViewModel vm) await vm.LoadGoalsAsync();
            }
        }

        private async void EditGoalMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is GoalViewModel selectedGoal)
            {
                var editGoalWindow = new EditGoalWindow();
                var goalService = GetService<IGoalService>();
                editGoalWindow.DataContext = new AddEditGoalViewModel(goalService, selectedGoal.GetGoalModel());

                if (VisualRoot is Window parentWindow)
                {
                    await editGoalWindow.ShowDialog(parentWindow);
                    if (DataContext is MainWindowViewModel vm) await vm.LoadGoalsAsync();
                }
            }
        }

        private async void AddDepositMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is GoalViewModel selectedGoal)
            {
                var depositWindow = new DepositWindow();
                var depositService = GetService<IDepositService>();
                var goalService = GetService<IGoalService>();

                var vm = new DepositViewModel(selectedGoal.GetGoalModel(), depositService, goalService);

                // --- FIX: ЖИВОЕ ОБНОВЛЕНИЕ ---
                // Подписываемся на событие обновления ВНУТРИ окна
                vm.OnProgressUpdated += async () =>
                {
                    if (DataContext is MainWindowViewModel mainVm)
                    {
                        // Обновляем список в главном окне, не дожидаясь закрытия диалога
                        await mainVm.LoadGoalsAsync();
                    }
                };
                // -----------------------------

                depositWindow.DataContext = vm;

                if (VisualRoot is Window parentWindow)
                {
                    await depositWindow.ShowDialog(parentWindow);
                    // На всякий случай обновляем и после закрытия
                    if (DataContext is MainWindowViewModel mainVm) await mainVm.LoadGoalsAsync();
                }
            }
        }
    }
}