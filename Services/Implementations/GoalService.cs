using FinanceFlow.Data;
using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceFlow.Services.Implementations
{
    public class GoalService : IGoalService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GoalService> _logger;

        public GoalService(AppDbContext context, ILogger<GoalService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Goal>> GetAllGoalsAsync()
        {
            try
            {
                var goals = await _context.Goals
                    .Include(g => g.GoalCategory) // Загружаем связанные категории
                    .Include(g => g.Deposits) // Загружаем историю пополнений
                    .OrderByDescending(g => g.CreatedAt) // Сортирую по дате создания (новые сначала)
                    .ToListAsync();

                return goals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка целей");
                return new List<Goal>();
            }
        }

        public async Task<Goal?> GetGoalByIdAsync(int goalId)
        {
            try
            {
                // Получаю конкретную цель для формы редактирования или просмотра деталей
                return await _context.Goals
                    .Include(g => g.GoalCategory)
                    .Include(g => g.Deposits)
                    .FirstOrDefaultAsync(g => g.GoalId == goalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении цели по ID: {GoalId}", goalId);
                return null;
            }
        }

        public async Task<(bool success, string message)> AddGoalAsync(Goal goal)
        {
            try
            {
                // Сначала проверяю, что все данные корректные
                var validationResult = ValidateGoal(goal);
                if (!validationResult.success) return validationResult;

                // Убеждаюсь, что выбранная категория реально существует в базе
                var categoryExists = await _context.GoalCategories.AnyAsync(c => c.CategoryId == goal.CategoryId);
                if (!categoryExists) return (false, "Категория не существует");

                // PostgreSQL капризничает с DateTime, поэтому явно указываю Unspecified
                goal.StartDate = DateTime.SpecifyKind(goal.StartDate, DateTimeKind.Unspecified);
                goal.EndDate = DateTime.SpecifyKind(goal.EndDate, DateTimeKind.Unspecified);
                goal.CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Создана цель: {GoalTitle}", goal.Title);
                return (true, "Цель успешно создана");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании цели");
                return (false, "Ошибка сохранения");
            }
        }

        public async Task<(bool success, string message)> UpdateGoalAsync(Goal goal)
        {
            try
            {
                // Нахожу существующую цель - обновляю только её, а не создаю новую
                var existingGoal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goal.GoalId);
                if (existingGoal == null) return (false, "Цель не найдена");

                // Проверяю новые данные перед сохранением
                var validationResult = ValidateGoal(goal);
                if (!validationResult.success) return validationResult;

                // Обновляю только нужные поля, оставляя системные данные без изменений
                existingGoal.Title = goal.Title;
                existingGoal.TargetAmount = goal.TargetAmount;
                existingGoal.CurrentAmount = goal.CurrentAmount; // Обновляем и текущую сумму если надо

                // Нормализация дат
                existingGoal.StartDate = DateTime.SpecifyKind(goal.StartDate, DateTimeKind.Unspecified);
                existingGoal.EndDate = DateTime.SpecifyKind(goal.EndDate, DateTimeKind.Unspecified);

                existingGoal.Description = goal.Description;
                existingGoal.Priority = goal.Priority;
                existingGoal.CategoryId = goal.CategoryId;
                existingGoal.ImagePath = goal.ImagePath;

                await _context.SaveChangesAsync();
                return (true, "Цель обновлена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении цели");
                return (false, "Ошибка обновления");
            }
        }

        public async Task<(bool success, string message)> DeleteGoalAsync(int goalId)
        {
            try
            {
                var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId);
                if (goal == null) return (false, "Цель не найдена");

                // В базе настроено каскадное удаление, так что все 
                // пополнения удалятся автоматически
                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
                return (true, "Цель удалена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления цели");
                return (false, "Ошибка удаления");
            }
        }

        public async Task<List<GoalCategory>> GetCategoriesAsync()
        {
            // Категории возвращаю отсортированными по порядку
            return await _context.GoalCategories.OrderBy(c => c.SortOrder).ToListAsync();
        }

        public async Task<List<Goal>> GetGoalsByCategoryAsync(int categoryId)
        {
            // Использую для фильтрации целей по категориям в аналитике
            return await _context.Goals
                .Include(g => g.GoalCategory)
                .Where(g => g.CategoryId == categoryId)
                .ToListAsync();
        }

        private (bool success, string message) ValidateGoal(Goal goal)
        {
            if (string.IsNullOrWhiteSpace(goal.Title)) return (false, "Название не может быть пустым");
            if (goal.TargetAmount <= 0) return (false, "Целевая сумма должна быть > 0");
            if (goal.EndDate <= goal.StartDate) return (false, "Дата окончания должна быть позже даты начала");
            return (true, string.Empty);
        }
    }
}