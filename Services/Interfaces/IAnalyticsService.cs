using FinanceFlow.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceFlow.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsSummaryDto> GetGeneralStatisticsAsync();
        Task<List<CategoryDistributionItem>> GetCategoryDistributionAsync();
        Task<List<Goal>> GetUpcomingDeadlinesAsync(int count = 3);
        Task<string> GeneratePdfReportAsync(string reportType);
    }

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
        public string Icon { get; set; } = string.Empty; // <-- ДОБАВЛЕНО
        public string Color { get; set; } = string.Empty;
        public int GoalsCount { get; set; }
        public double Percentage { get; set; }
    }
}