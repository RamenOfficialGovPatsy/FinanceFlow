namespace FinanceFlow.Models
{
    public class GoalCategory
    {
        public int CategoryId { get; set; } // Уникальный идентификатор категории
        public string Name { get; set; } = string.Empty; // Категории (Техника, Путешествия и т.д.)
        public string Icon { get; set; } = string.Empty; // Иконка категории в виде эмодзи
        public string Color { get; set; } = string.Empty;  // Цвет категории в HEX формате
        public int SortOrder { get; set; } // Порядок сортировки в интерфейсе
        public bool IsActive { get; set; } = true; // Флаг активности категории
        public DateTime CreatedAt { get; set; } // Дата создания категории

        // Навигационное свойство - цели этой категории
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    }
}