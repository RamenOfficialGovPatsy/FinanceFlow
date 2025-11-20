using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class AnalyticsViewModel : ViewModelBase
    {
        // Сервис может быть null в режиме дизайнера (конструктор без параметров)
        private readonly IAnalyticsService? _analyticsService;

        // --- Свойства статистики ---

        private int _totalGoals;
        public int TotalGoals
        {
            get => _totalGoals;
            set => SetProperty(ref _totalGoals, value);
        }

        private int _completedGoals;
        public int CompletedGoals
        {
            get => _completedGoals;
            set
            {
                if (SetProperty(ref _completedGoals, value))
                {
                    OnPropertyChanged(nameof(CompletedGoalsText));
                }
            }
        }

        // Форматированный текст: "3 (37.5%)"
        public string CompletedGoalsText => $"{CompletedGoals} ({GetPercentage(CompletedGoals, TotalGoals)}%)";

        private int _inProgressGoals;
        public int InProgressGoals
        {
            get => _inProgressGoals;
            set => SetProperty(ref _inProgressGoals, value);
        }

        private int _overdueGoals;
        public int OverdueGoals
        {
            get => _overdueGoals;
            set => SetProperty(ref _overdueGoals, value);
        }

        private decimal _totalTargetAmount;
        public decimal TotalTargetAmount
        {
            get => _totalTargetAmount;
            set
            {
                if (SetProperty(ref _totalTargetAmount, value))
                {
                    OnPropertyChanged(nameof(TotalTargetAmountText));
                }
            }
        }

        public string TotalTargetAmountText => $"{TotalTargetAmount:N0} ₽";

        private decimal _totalCurrentAmount;
        public decimal TotalCurrentAmount
        {
            get => _totalCurrentAmount;
            set
            {
                if (SetProperty(ref _totalCurrentAmount, value))
                {
                    OnPropertyChanged(nameof(AccumulatedText));
                }
            }
        }

        private double _averageProgress;
        public double AverageProgress
        {
            get => _averageProgress;
            set
            {
                if (SetProperty(ref _averageProgress, value))
                {
                    OnPropertyChanged(nameof(AccumulatedText));
                }
            }
        }

        // Форматированный текст: "215 000 ₽ (44.3%)"
        public string AccumulatedText => $"{TotalCurrentAmount:N0} ₽ ({AverageProgress:F1}%)";

        // --- Коллекции ---

        // Данные для диаграммы
        public ObservableCollection<CategoryDistributionItem> CategoryDistribution { get; } = new();

        // Данные для списка дедлайнов
        public ObservableCollection<GoalViewModel> UpcomingDeadlines { get; } = new();

        // --- Команды ---

        public ICommand GenerateReportCommand { get; }
        public ICommand CloseCommand { get; }

        // --- Конструкторы ---

        // 1. Конструктор для Design-time (чтобы XAML-дизайнер не падал и показывал данные)
        public AnalyticsViewModel()
        {
            _analyticsService = null;

            // Инициализируем команды-заглушки (чтобы избежать Warning CS8618)
            GenerateReportCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            CloseCommand = new AsyncRelayCommand(() => Task.CompletedTask);

            // Заполняем моковыми данными для превью
            TotalGoals = 8;
            CompletedGoals = 3;
            InProgressGoals = 4;
            OverdueGoals = 1;
            TotalTargetAmount = 485000;
            TotalCurrentAmount = 215000;
            AverageProgress = 44.3;
        }

        // 2. Основной конструктор (Runtime)
        public AnalyticsViewModel(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));

            GenerateReportCommand = new AsyncRelayCommand(GeneratePdfReportAsync);
            CloseCommand = new AsyncRelayCommand(() => Task.CompletedTask); // Закрытие обычно обрабатывается View, здесь заглушка

            // Запускаем загрузку данных (fire-and-forget для конструктора)
            _ = LoadAnalyticsDataAsync();
        }

        // --- Логика ---

        public async Task LoadAnalyticsDataAsync()
        {
            if (_analyticsService == null) return;

            // 1. Получаем общую статистику
            var stats = await _analyticsService.GetGeneralStatisticsAsync();

            TotalGoals = stats.TotalGoals;
            CompletedGoals = stats.CompletedGoals;
            InProgressGoals = stats.InProgressGoals;
            OverdueGoals = stats.OverdueGoals;
            TotalTargetAmount = stats.TotalTargetAmount;
            TotalCurrentAmount = stats.TotalCurrentAmount;
            AverageProgress = stats.AverageProgress;

            // 2. Получаем распределение по категориям
            var distribution = await _analyticsService.GetCategoryDistributionAsync();
            CategoryDistribution.Clear();
            foreach (var item in distribution)
            {
                CategoryDistribution.Add(item);
            }

            // 3. Получаем дедлайны
            var deadlines = await _analyticsService.GetUpcomingDeadlinesAsync(3);
            UpcomingDeadlines.Clear();
            foreach (var goal in deadlines)
            {
                // Оборачиваем модель в ViewModel для удобного отображения
                UpcomingDeadlines.Add(new GoalViewModel(goal));
            }
        }

        private async Task GeneratePdfReportAsync()
        {
            if (_analyticsService == null) return;

            // Здесь будет логика генерации PDF через QuestPDF
            // Пока просто создаем запись в БД
            var reportRecord = await _analyticsService.GenerateReportRecordAsync("custom");
            Console.WriteLine($"Отчет создан: ID {reportRecord.ReportId}");
        }

        // Вспомогательный метод для расчета процентов в строке
        private string GetPercentage(int value, int total)
        {
            if (total == 0) return "0";
            return ((double)value / total * 100).ToString("F1");
        }
    }
}