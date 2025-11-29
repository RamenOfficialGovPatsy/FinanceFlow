namespace FinanceFlow.Models
{
    public class AnalyticsReport
    {
        public int ReportId { get; set; } // Уникальный идентификатор отчета
        public string ReportType { get; set; } = "monthly"; // Тип отчета: ежемесячно, 
        // ежеквартально, ежегодно или индивидуально
        public DateTime ReportDate { get; set; } // Дата, на которую сформирован отчет
        public int TotalGoals { get; set; } // Общее количество целей
        public int CompletedGoals { get; set; } // Количество завершенных целей
        public decimal TotalTargetAmount { get; set; } // Сумма всех целевых сумм
        public decimal TotalCurrentAmount { get; set; } // Сумма всех текущих накоплений
        public decimal AverageProgress { get; set; } // Средний прогресс по всем целям в процентах
        public DateTime GeneratedAt { get; set; } // Дата и время генерации отчета
    }
}