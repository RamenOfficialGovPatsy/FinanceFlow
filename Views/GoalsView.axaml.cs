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

        // Вспомогательный метод для получения сервисов из DI
        private T GetService<T>() where T : notnull
        {
            var app = (App)Application.Current!;
            return app.Services!.GetRequiredService<T>();
        }

        private async void AnalyticsButton_Click(object? sender, RoutedEventArgs e)
        {
            var analyticsWindow = new AnalyticsWindow();

            // --- ИЗМЕНЕНИЕ ЗДЕСЬ ---
            // Получаем ViewModel из контейнера.
            // Контейнер сам создаст AnalyticsService и передаст его внутрь ViewModel.
            var vm = GetService<AnalyticsViewModel>();

            // Устанавливаем DataContext ("подключаем данные к окну")
            analyticsWindow.DataContext = vm;
            // -----------------------

            if (VisualRoot is Window parentWindow)
            {
                await analyticsWindow.ShowDialog(parentWindow);
            }
        }

        private async void AddGoalButton_Click(object? sender, RoutedEventArgs e)
        {
            var addGoalWindow = new AddGoalWindow();

            // Здесь тоже можно было бы использовать GetService<AddEditGoalViewModel>(),
            // но пока оставим как было, чтобы не менять слишком много сразу.
            // Главное, что мы получаем IGoalService через DI.
            var goalService = GetService<IGoalService>();
            addGoalWindow.DataContext = new AddEditGoalViewModel(goalService);

            if (VisualRoot is Window parentWindow)
            {
                await addGoalWindow.ShowDialog(parentWindow);
                if (DataContext is MainWindowViewModel vm) await vm.LoadGoalsAsync();
            }
        }

        private async void EditGoalMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is GoalViewModel selectedGoal)
            {
                var editGoalWindow = new EditGoalWindow();
                var goalService = GetService<IGoalService>();

                // Передаем модель для редактирования
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

                // Для депозитов пока создаем вручную, так как нужно передать конкретную цель
                var vm = new DepositViewModel(selectedGoal.GetGoalModel(), depositService, goalService);

                vm.OnProgressUpdated += async () =>
                {
                    if (DataContext is MainWindowViewModel mainVm)
                    {
                        await mainVm.LoadGoalsAsync();
                    }
                };

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