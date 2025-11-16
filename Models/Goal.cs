using System;
using System.Collections.Generic;

namespace FinanceFlow.Models
{
    public class Goal
    {
        public int GoalId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ImagePath { get; set; }
        public string? Description { get; set; }
        public int Priority { get; set; } = 2;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public GoalCategory GoalCategory { get; set; } = null!;
        public ICollection<GoalDeposit> Deposits { get; set; } = new List<GoalDeposit>();
    }
}