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

        // Вспомогательный метод для получения сервисов из DI-контейнера
        // Позволяет избежать жестких зависимостей и использовать внедрение зависимостей
        private T GetService<T>() where T : notnull
        {
            var app = (App)Application.Current!;
            return app.Services!.GetRequiredService<T>();
        }

        private async void AnalyticsButton_Click(object? sender, RoutedEventArgs e)
        {
            // Создаем новое окно аналитики
            var analyticsWindow = new AnalyticsWindow();

            // Получаем ViewModel аналитики из DI-контейнера
            var vm = GetService<AnalyticsViewModel>();

            // Связываем ViewModel с окном - теперь данные будут отображаться
            analyticsWindow.DataContext = vm;

            // Показываем окно как модальное поверх родительского окна
            if (VisualRoot is Window parentWindow)
            {
                await analyticsWindow.ShowDialog(parentWindow);
            }
        }

        private async void AddGoalButton_Click(object? sender, RoutedEventArgs e)
        {
            var addGoalWindow = new AddGoalWindow();

            // Получаем сервис работы с целями из DI-контейнера
            var goalService = GetService<IGoalService>();

            // Создаем ViewModel для окна добавления цели
            addGoalWindow.DataContext = new AddEditGoalViewModel(goalService);

            // Показываем окно как модальное
            if (VisualRoot is Window parentWindow)
            {
                await addGoalWindow.ShowDialog(parentWindow);

                // После закрытия окна обновляем список целей
                if (DataContext is MainWindowViewModel vm) await vm.LoadGoalsAsync();
            }
        }

        private async void EditGoalMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            // Проверяем что событие вызвано из MenuItem и есть выбранная цель
            if (sender is MenuItem menuItem && menuItem.Tag is GoalViewModel selectedGoal)
            {
                var editGoalWindow = new EditGoalWindow();

                // Получаем сервис работы с целями
                var goalService = GetService<IGoalService>();

                // Создаем ViewModel для редактирования - передаем существующую цель
                editGoalWindow.DataContext = new AddEditGoalViewModel(goalService, selectedGoal.GetGoalModel());

                if (VisualRoot is Window parentWindow)
                {
                    await editGoalWindow.ShowDialog(parentWindow);

                    // После закрытия обновляем список целей
                    if (DataContext is MainWindowViewModel vm) await vm.LoadGoalsAsync();
                }
            }
        }

        private async void AddDepositMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            // Проверяем что есть выбранная цель для пополнения
            if (sender is MenuItem menuItem && menuItem.Tag is GoalViewModel selectedGoal)
            {
                var depositWindow = new DepositWindow();

                // Получаем необходимые сервисы из DI-контейнера
                var depositService = GetService<IDepositService>();
                var goalService = GetService<IGoalService>();

                // Создаем ViewModel для работы с пополнениями
                var vm = new DepositViewModel(selectedGoal.GetGoalModel(), depositService, goalService);

                // Подписываемся на событие обновления прогресса
                vm.OnProgressUpdated += async () =>
                {
                    if (DataContext is MainWindowViewModel mainVm)
                    {
                        await mainVm.LoadGoalsAsync();
                    }
                };

                // Связываем ViewModel с окном
                depositWindow.DataContext = vm;

                if (VisualRoot is Window parentWindow)
                {
                    await depositWindow.ShowDialog(parentWindow);
                    if (DataContext is MainWindowViewModel mainVm) await mainVm.LoadGoalsAsync();
                }
            }
        }
    }
}