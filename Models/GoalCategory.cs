using System;
using System.Collections.Generic;

namespace FinanceFlow.Models
{
    public class GoalCategory
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    }
}