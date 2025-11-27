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
                    .Include(g => g.GoalCategory)
                    .Include(g => g.Deposits)
                    .OrderByDescending(g => g.CreatedAt)
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
                // ЧИСТЫЙ МЕТОД (Убрали костыли с MinValue)
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
                var validationResult = ValidateGoal(goal);
                if (!validationResult.success) return validationResult;

                var categoryExists = await _context.GoalCategories.AnyAsync(c => c.CategoryId == goal.CategoryId);
                if (!categoryExists) return (false, "Категория не существует");

                // Упрощенная нормализация (оставляем только Unspecified для надежности)
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
                var existingGoal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goal.GoalId);
                if (existingGoal == null) return (false, "Цель не найдена");

                var validationResult = ValidateGoal(goal);
                if (!validationResult.success) return validationResult;

                existingGoal.Title = goal.Title;
                existingGoal.TargetAmount = goal.TargetAmount;
                existingGoal.CurrentAmount = goal.CurrentAmount; // Обновляем и текущую сумму если надо

                // Нормализация
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
            return await _context.GoalCategories.OrderBy(c => c.SortOrder).ToListAsync();
        }

        public async Task<List<Goal>> GetGoalsByCategoryAsync(int categoryId)
        {
            return await _context.Goals
                .Include(g => g.GoalCategory)
                .Where(g => g.CategoryId == categoryId)
                .ToListAsync();
        }

        private (bool success, string message) ValidateGoal(Goal goal)
        {
            if (string.IsNullOrWhiteSpace(goal.Title)) return (false, "Название не может быть пустым");
            if (goal.TargetAmount <= 0) return (false, "Целевая сумма должна быть > 0");
            // Проверку дат убрали из БД, но здесь можно оставить логическую
            if (goal.EndDate <= goal.StartDate) return (false, "Дата окончания должна быть позже даты начала");
            return (true, string.Empty);
        }
    }
}