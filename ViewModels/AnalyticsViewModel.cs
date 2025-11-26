using Avalonia.Media;
using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class AnalyticsViewModel : ViewModelBase
    {
        private readonly IAnalyticsService? _analyticsService;

        // Жесткая палитра цветов по ТЗ
        private readonly string[] _palette = new[]
        {
            "#311B92", // Темно-фиолетовый
            "#065F46", // Темно-зеленый
            "#B45309", // Темно-оранжевый
            "#880E4F", // Темно-красный
            "#1A237E", // Темно-синий
            "#006064"  // Морская волна
        };

        private int _totalGoals;
        public int TotalGoals { get => _totalGoals; set => SetProperty(ref _totalGoals, value); }

        private int _completedGoals;
        public int CompletedGoals
        {
            get => _completedGoals;
            set { if (SetProperty(ref _completedGoals, value)) OnPropertyChanged(nameof(CompletedGoalsText)); }
        }
        public string CompletedGoalsText => $"{CompletedGoals} ({GetPercentage(CompletedGoals, TotalGoals)}%)";

        private int _inProgressGoals;
        public int InProgressGoals { get => _inProgressGoals; set => SetProperty(ref _inProgressGoals, value); }

        private int _overdueGoals;
        public int OverdueGoals { get => _overdueGoals; set => SetProperty(ref _overdueGoals, value); }

        private decimal _totalTargetAmount;
        public decimal TotalTargetAmount
        {
            get => _totalTargetAmount;
            set { if (SetProperty(ref _totalTargetAmount, value)) OnPropertyChanged(nameof(TotalTargetAmountText)); }
        }
        public string TotalTargetAmountText => $"{TotalTargetAmount:N0} ₽";

        private decimal _totalCurrentAmount;
        public decimal TotalCurrentAmount
        {
            get => _totalCurrentAmount;
            set { if (SetProperty(ref _totalCurrentAmount, value)) OnPropertyChanged(nameof(AccumulatedText)); }
        }

        private double _averageProgress;
        public double AverageProgress
        {
            get => _averageProgress;
            set { if (SetProperty(ref _averageProgress, value)) OnPropertyChanged(nameof(AccumulatedText)); }
        }
        public string AccumulatedText => $"{TotalCurrentAmount:N0} ₽ ({AverageProgress:F1}%)";

        private List<double> _chartValues = new();
        public List<double> ChartValues
        {
            get => _chartValues;
            set => SetProperty(ref _chartValues, value);
        }

        private List<Color> _chartColors = new();
        public List<Color> ChartColors
        {
            get => _chartColors;
            set => SetProperty(ref _chartColors, value);
        }

        public ObservableCollection<CategoryDistributionItem> CategoryLegend { get; } = new();
        public ObservableCollection<GoalViewModel> UpcomingDeadlines { get; } = new();

        public ICommand GenerateReportCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? RequestClose;

        public AnalyticsViewModel()
        {
            _analyticsService = null;
            GenerateReportCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            CloseCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        }

        public AnalyticsViewModel(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));

            GenerateReportCommand = new AsyncRelayCommand(GeneratePdfReportAsync);
            CloseCommand = new AsyncRelayCommand(() =>
            {
                RequestClose?.Invoke();
                return Task.CompletedTask;
            });

            _ = LoadAnalyticsDataAsync();
        }

        public async Task LoadAnalyticsDataAsync()
        {
            if (_analyticsService == null) return;

            var stats = await _analyticsService.GetGeneralStatisticsAsync();
            TotalGoals = stats.TotalGoals;
            CompletedGoals = stats.CompletedGoals;
            InProgressGoals = stats.InProgressGoals;
            OverdueGoals = stats.OverdueGoals;
            TotalTargetAmount = stats.TotalTargetAmount;
            TotalCurrentAmount = stats.TotalCurrentAmount;
            AverageProgress = stats.AverageProgress;

            var distribution = await _analyticsService.GetCategoryDistributionAsync();

            CategoryLegend.Clear();
            var newValues = new List<double>();
            var newColors = new List<Color>();

            int colorIndex = 0;
            foreach (var item in distribution)
            {
                // Присваиваем цвет сегменту из нашей палитры по очереди
                string hexColor = _palette[colorIndex % _palette.Length];

                // ВАЖНО: Обновляем цвет в самом элементе, чтобы легенда совпадала с графиком
                item.Color = hexColor;

                CategoryLegend.Add(item);
                newValues.Add(item.Percentage);

                if (Color.TryParse(hexColor, out Color color))
                    newColors.Add(color);
                else
                    newColors.Add(Colors.Gray);

                colorIndex++;
            }

            ChartValues = newValues;
            ChartColors = newColors;

            var deadlines = await _analyticsService.GetUpcomingDeadlinesAsync(3);
            UpcomingDeadlines.Clear();
            foreach (var goal in deadlines)
            {
                UpcomingDeadlines.Add(new GoalViewModel(goal));
            }
        }

        private async Task GeneratePdfReportAsync()
        {
            if (_analyticsService == null) return;
            try
            {
                string filePath = await _analyticsService.GeneratePdfReportAsync("custom");
                if (!string.IsNullOrEmpty(filePath)) OpenUrl(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка генерации отчета: {ex.Message}");
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch
            {
                if (OperatingSystem.IsLinux()) Process.Start("xdg-open", url);
            }
        }

        private string GetPercentage(int value, int total)
        {
            if (total == 0) return "0";
            return ((double)value / total * 100).ToString("F1");
        }
    }
}