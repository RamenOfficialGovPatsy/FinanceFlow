using System;

namespace FinanceFlow.Models
{
    public class GoalDeposit
    {
        public int DepositId { get; set; }
        public int GoalId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DepositDate { get; set; }
        public string? Comment { get; set; }
        public string DepositType { get; set; } = "regular";

        public Goal Goal { get; set; } = null!;
    }
}