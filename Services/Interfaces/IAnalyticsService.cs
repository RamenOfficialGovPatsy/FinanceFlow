using FinanceFlow.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceFlow.Services.Interfaces
{
    public interface IAnalyticsService
    {
        // 1. Получение общей сводки (всего целей, выполнено, суммы и т.д.)
        Task<AnalyticsSummaryDto> GetGeneralStatisticsAsync();

        // 2. Получение данных для круговой диаграммы (распределение по категориям)
        Task<List<CategoryDistributionItem>> GetCategoryDistributionAsync();

        // 3. Получение списка ближайших дедлайнов (по умолчанию 3 штуки)
        Task<List<Goal>> GetUpcomingDeadlinesAsync(int count = 3);

        // 4. Генерация и сохранение записи отчета в БД
        Task<AnalyticsReport> GenerateReportRecordAsync(string reportType);
    }

    // --- DTO классы для передачи данных ---

    public class AnalyticsSummaryDto
    {
        public int TotalGoals { get; set; }
        public int CompletedGoals { get; set; }
        public int InProgressGoals { get; set; }
        public int OverdueGoals { get; set; }
        public decimal TotalTargetAmount { get; set; }
        public decimal TotalCurrentAmount { get; set; }
        public double AverageProgress { get; set; }
    }

    public class CategoryDistributionItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty; // HEX цвет
        public int GoalsCount { get; set; }
        public double Percentage { get; set; }
    }
}