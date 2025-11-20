using FinanceFlow.Data;
using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceFlow.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsSummaryDto> GetGeneralStatisticsAsync()
        {
            var goals = await _context.Goals.ToListAsync();
            var summary = new AnalyticsSummaryDto();

            if (!goals.Any())
                return summary;

            summary.TotalGoals = goals.Count;
            summary.CompletedGoals = goals.Count(g => g.IsCompleted);

            // Просроченные: Не выполнены И дата окончания уже прошла
            summary.OverdueGoals = goals.Count(g => !g.IsCompleted && g.EndDate < DateTime.Today);

            // В процессе: Не выполнены И дата окончания еще не прошла (или сегодня)
            summary.InProgressGoals = goals.Count(g => !g.IsCompleted && g.EndDate >= DateTime.Today);

            summary.TotalTargetAmount = goals.Sum(g => g.TargetAmount);
            summary.TotalCurrentAmount = goals.Sum(g => g.CurrentAmount);

            // Средний прогресс (Считаем как отношение: Всего Накоплено / Всего Нужно)
            if (summary.TotalTargetAmount > 0)
            {
                summary.AverageProgress = (double)(summary.TotalCurrentAmount / summary.TotalTargetAmount) * 100;

                // Ограничиваем 100%, если вдруг перевыполнили
                if (summary.AverageProgress > 100) summary.AverageProgress = 100;
            }

            return summary;
        }

        public async Task<List<CategoryDistributionItem>> GetCategoryDistributionAsync()
        {
            // Загружаем цели вместе с категориями
            var goals = await _context.Goals
                .Include(g => g.GoalCategory)
                .ToListAsync();

            if (!goals.Any())
                return new List<CategoryDistributionItem>();

            var totalGoals = goals.Count;

            // Группируем цели по категориям и считаем статистику
            var distribution = goals
                .GroupBy(g => g.GoalCategory)
                .Select(g => new CategoryDistributionItem
                {
                    CategoryName = g.Key != null ? g.Key.Name : "Без категории",
                    Color = g.Key != null ? g.Key.Color : "#6B7280",
                    GoalsCount = g.Count(),
                    Percentage = Math.Round((double)g.Count() / totalGoals * 100, 1)
                })
                .OrderByDescending(x => x.Percentage) // Сортируем: самые популярные сверху
                .ToList();

            return distribution;
        }

        public async Task<List<Goal>> GetUpcomingDeadlinesAsync(int count = 3)
        {
            // Берем невыполненные цели, сортируем по дате окончания (ближайшие сверху)
            return await _context.Goals
                .Include(g => g.GoalCategory) // Подгружаем категорию для иконок
                .Where(g => !g.IsCompleted)
                .OrderBy(g => g.EndDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<AnalyticsReport> GenerateReportRecordAsync(string reportType)
        {
            // 1. Получаем актуальную статистику
            var stats = await GetGeneralStatisticsAsync();

            // 2. Создаем модель отчета
            var report = new AnalyticsReport
            {
                ReportType = reportType,
                ReportDate = DateTime.Today,
                GeneratedAt = DateTime.Now,
                TotalGoals = stats.TotalGoals,
                CompletedGoals = stats.CompletedGoals,
                TotalTargetAmount = stats.TotalTargetAmount,
                TotalCurrentAmount = stats.TotalCurrentAmount,
                AverageProgress = (decimal)stats.AverageProgress
            };

            // 3. Сохраняем запись в БД
            _context.AnalyticsReports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }
    }
}