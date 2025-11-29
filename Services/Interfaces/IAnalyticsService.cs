using FinanceFlow.Models;

namespace FinanceFlow.Services.Interfaces
{
    public interface IAnalyticsService
    {
        // Получение общей статистики по всем целям
        Task<AnalyticsSummaryDto> GetGeneralStatisticsAsync();

        // Получение распределения целей по категориям для круговой диаграммы
        Task<List<CategoryDistributionItem>> GetCategoryDistributionAsync();

        // Получение списка ближайших дедлайнов
        Task<List<Goal>> GetUpcomingDeadlinesAsync(int count = 3);

        // Генерация PDF отчета по указанному типу
        Task<string> GeneratePdfReportAsync(string reportType);
    }

    // DTO для передачи общей статистики
    public class AnalyticsSummaryDto
    {
        public int TotalGoals { get; set; } // Всего целей
        public int CompletedGoals { get; set; } // Завершенные цели
        public int InProgressGoals { get; set; } // Цели в процессе
        public int OverdueGoals { get; set; } // Просроченные цели
        public decimal TotalTargetAmount { get; set; } // Общая целевая сумма
        public decimal TotalCurrentAmount { get; set; } // Общая накопленная сумма
        public double AverageProgress { get; set; } // Средний прогресс в процентах
    }

    // Элемент распределения по категориям для диаграммы
    public class CategoryDistributionItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty; // Иконка категории (эмодзи)
        public string Color { get; set; } = string.Empty; // Цвет категории в HEX
        public int GoalsCount { get; set; } // Количество целей в категории
        public double Percentage { get; set; } // Процентное соотношение
    }
}