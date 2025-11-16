using System;
using System.ComponentModel.DataAnnotations; // <-- 1. ДОБАВЛЯЕМ ЭТОТ USING

namespace FinanceFlow.Models
{
    public class AnalyticsReport
    {
        public int ReportId { get; set; }

        public string ReportType { get; set; } = "monthly";
        public DateTime ReportDate { get; set; }
        public int TotalGoals { get; set; }
        public int CompletedGoals { get; set; }
        public decimal TotalTargetAmount { get; set; }
        public decimal TotalCurrentAmount { get; set; }
        public decimal AverageProgress { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}