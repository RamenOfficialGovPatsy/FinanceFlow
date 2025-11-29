using Avalonia.Media;
using FinanceFlow.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class AnalyticsViewModel : ViewModelBase
    {
        private readonly IAnalyticsService? _analyticsService;

        // Фиксированная палитра из 30 насыщенных темных цветов для круговой диаграммы
        private readonly string[] _palette = new[]
        {
            // --- Красные и Бордовые ---
            "#B71C1C", // Red 900 (Глубокий красный)
            "#D50000", // Red A700 (Яркий насыщенный красный)
            "#880E4F", // Pink 900 (Темно-малиновый)
            "#C2185B", // Pink 700 (Насыщенный розовый)
            "#8B0000", // DarkRed (Классический темно-красный)

            // --- Фиолетовые и Индиго ---
            "#4A148C", // Purple 900 (Глубокий фиолетовый)
            "#7B1FA2", // Purple 700 (Насыщенный фиолетовый)
            "#311B92", // Deep Purple 900 (Темный ультрамарин)
            "#6200EA", // Deep Purple A700 (Яркий индиго)
            "#1A237E", // Indigo 900 (Темно-синий индиго)
            "#304FFE", // Indigo A700 (Яркий электрик)

            // --- Синие и Лазурные ---
            "#0D47A1", // Blue 900 (Темно-синий)
            "#1565C0", // Blue 800 (Насыщенный синий)
            "#01579B", // Light Blue 900 (Глубокий голубой)
            "#0277BD", // Light Blue 800 (Морской синий)
            "#006064", // Cyan 900 (Темная морская волна)

            // --- Бирюзовые и Тиловые ---
            "#004D40", // Teal 900 (Глубокий тиловый)
            "#00695C", // Teal 800 (Насыщенный тиловый)
            "#00BFA5", // Teal A700 (Яркая бирюза)
            "#1DE9B6", // Teal A400 (Неоновая бирюза - акцент)

            // --- Зеленые и Изумрудные ---
            "#1B5E20", // Green 900 (Лесной зеленый)
            "#2E7D32", // Green 800 (Травяной зеленый)
            "#00C853", // Green A700 (Яркий изумруд)
            "#33691E", // Light Green 900 (Оливково-зеленый)

            // --- Оранжевые и Янтарные (Глубокие) ---
            "#BF360C", // Deep Orange 900 (Кирпичный)
            "#D84315", // Deep Orange 800 (Насыщенный рыжий)
            "#E65100", // Orange 900 (Темно-оранжевый)
            "#FF6D00", // Orange A700 (Яркий оранжевый)
            "#F57F17", // Yellow 900 (Темно-янтарный/Золотой)
            "#FFD600"  // Yellow A700 (Насыщенный золотой)
        };

        // Основные статистические свойства с уведомлениями об изменении
        private int _totalGoals;
        public int TotalGoals { get => _totalGoals; set => SetProperty(ref _totalGoals, value); }

        private int _completedGoals;
        public int CompletedGoals
        {
            get => _completedGoals;
            set { if (SetProperty(ref _completedGoals, value)) OnPropertyChanged(nameof(CompletedGoalsText)); }
        }

        // Вычисляемое свойство для отображения завершенных целей с процентом
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

        // Отображаю накопленную сумму и общий прогресс в одной строке
        public string AccumulatedText => $"{TotalCurrentAmount:N0} ₽ ({AverageProgress:F1}%)";

        // Данные для круговой диаграммы - значения и цвета сегментов
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

        // Коллекции для отображения в интерфейсе
        public ObservableCollection<CategoryDistributionItem> CategoryLegend { get; } = new();
        public ObservableCollection<GoalViewModel> UpcomingDeadlines { get; } = new();

        // Команды для кнопок
        public ICommand GenerateReportCommand { get; }
        public ICommand CloseCommand { get; }

        public event Action? RequestClose;

        // Конструктор для дизайнера
        public AnalyticsViewModel()
        {
            _analyticsService = null;
            GenerateReportCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            CloseCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        }

        // Основной конструктор с зависимостями
        public AnalyticsViewModel(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));

            GenerateReportCommand = new AsyncRelayCommand(GeneratePdfReportAsync);
            CloseCommand = new AsyncRelayCommand(() =>
            {
                RequestClose?.Invoke();
                return Task.CompletedTask;
            });

            // Загружаю данные аналитики сразу при создании ViewModel
            _ = LoadAnalyticsDataAsync();
        }

        // Основной метод загрузки всех данных аналитики
        public async Task LoadAnalyticsDataAsync()
        {
            if (_analyticsService == null) return;

            // Загружаю общую статистику
            var stats = await _analyticsService.GetGeneralStatisticsAsync();
            TotalGoals = stats.TotalGoals;
            CompletedGoals = stats.CompletedGoals;
            InProgressGoals = stats.InProgressGoals;
            OverdueGoals = stats.OverdueGoals;
            TotalTargetAmount = stats.TotalTargetAmount;
            TotalCurrentAmount = stats.TotalCurrentAmount;
            AverageProgress = stats.AverageProgress;

            // Загружаю распределение по категориям для диаграммы
            var distribution = await _analyticsService.GetCategoryDistributionAsync();

            // Перемешиваю палитру, чтобы цвета каждый раз распределялись по-разному
            var random = new Random();
            var shuffledPalette = _palette.OrderBy(x => random.Next()).ToArray();

            // Очищаю предыдущие данные и готовлю новые для диаграммы
            CategoryLegend.Clear();
            var newValues = new List<double>();
            var newColors = new List<Color>();

            int colorIndex = 0;
            foreach (var item in distribution)
            {
                // Беру следующий цвет из перемешанной палитры
                string hexColor = shuffledPalette[colorIndex % shuffledPalette.Length];
                item.Color = hexColor;

                CategoryLegend.Add(item);
                newValues.Add(item.Percentage);

                // Паршу HEX в Color для Avalonia
                if (Color.TryParse(hexColor, out Color color))
                    newColors.Add(color);
                else
                    newColors.Add(Colors.Gray); // Запасной вариант если парсинг не удался

                colorIndex++;
            }

            // Обновляю данные диаграммы
            ChartValues = newValues;
            ChartColors = newColors;

            // Загружаю ближайшие дедлайны для отображения
            var deadlines = await _analyticsService.GetUpcomingDeadlinesAsync(3);
            UpcomingDeadlines.Clear();
            foreach (var goal in deadlines)
            {
                UpcomingDeadlines.Add(new GoalViewModel(goal));
            }
        }

        // Генерация PDF отчета с открытием после создания
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

        // Открываю файл или URL в стандартном приложении системы
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

        // Вспомогательный метод для расчета процентов
        private string GetPercentage(int value, int total)
        {
            if (total == 0) return "0";
            return ((double)value / total * 100).ToString("F1");
        }
    }
}