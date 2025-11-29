using FinanceFlow.Models;

namespace FinanceFlow.Services.Interfaces
{
    public interface IGoalService
    {
        // Получение списка всех целей
        Task<List<Goal>> GetAllGoalsAsync();

        // Получение конкретной цели по идентификатору
        Task<Goal?> GetGoalByIdAsync(int goalId);

        // Добавление новой цели с валидацией
        Task<(bool success, string message)> AddGoalAsync(Goal goal);

        // Обновление существующей цели
        Task<(bool success, string message)> UpdateGoalAsync(Goal goal);

        // Удаление цели по идентификатору
        Task<(bool success, string message)> DeleteGoalAsync(int goalId);

        // Получение целей по категории
        Task<List<Goal>> GetGoalsByCategoryAsync(int categoryId);

        // Получение списка всех категорий целей
        Task<List<GoalCategory>> GetCategoriesAsync();
    }
}