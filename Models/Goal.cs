namespace FinanceFlow.Models
{
    public class Goal
    {
        public int GoalId { get; set; } // Уникальный идентификатор цели
        public int CategoryId { get; set; } // ID категории цели
        public string Title { get; set; } = string.Empty; // Название цели
        public decimal TargetAmount { get; set; } // Целевая сумма для накопления
        public decimal CurrentAmount { get; set; } // Текущая накопленная сумма
        public DateTime StartDate { get; set; } // Дата начала накопления
        public DateTime EndDate { get; set; } // Планируемая дата завершения
        public string? ImagePath { get; set; } // Путь к изображению цели (опционально)
        public string? Description { get; set; } // Описание цели (опционально)
        public int Priority { get; set; } = 2; // Приоритет: 1-высокий, 2-средний, 3-низкий
        public bool IsCompleted { get; set; } // Флаг завершения цели
        public DateTime CreatedAt { get; set; } // Дата создания цели

        public GoalCategory GoalCategory { get; set; } = null!; // Категория цели

        // История пополнений
        public ICollection<GoalDeposit> Deposits { get; set; } = new List<GoalDeposit>();
    }
}