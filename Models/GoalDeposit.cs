namespace FinanceFlow.Models
{
    public class GoalDeposit
    {
        public int DepositId { get; set; } // Уникальный идентификатор пополнения
        public int GoalId { get; set; } // ID цели, к которой относится пополнение
        public decimal Amount { get; set; } // Сумма пополнения
        public DateTime DepositDate { get; set; } // Дата внесения пополнения
        public string? Comment { get; set; } // Комментарий к пополнению (опционально)
        public string DepositType { get; set; } = "regular"; // Тип пополнения: обычное, зарплата, фриланм, бонус и тд

        // Навигационное свойство - цель, к которой относится пополнение
        public Goal Goal { get; set; } = null!;
    }
}