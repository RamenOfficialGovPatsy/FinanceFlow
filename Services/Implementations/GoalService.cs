using FinanceFlow.Data;
using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceFlow.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для управления целями
    /// </summary>
    public class GoalService : IGoalService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GoalService> _logger;

        public GoalService(AppDbContext context, ILogger<GoalService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<List<Goal>> GetAllGoalsAsync()
        {
            try
            {
                return await _context.Goals
                    .Include(g => g.GoalCategory)
                    .Include(g => g.Deposits)
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка целей");
                return new List<Goal>();
            }
        }

        /// <inheritdoc/>
        public async Task<Goal?> GetGoalByIdAsync(int goalId)
        {
            try
            {
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

        /// <inheritdoc/>
        public async Task<(bool success, string message)> AddGoalAsync(Goal goal)
        {
            try
            {
                // Валидация бизнес-правил
                var validationResult = ValidateGoal(goal);
                if (!validationResult.success)
                    return validationResult;

                // Проверка существования категории
                var categoryExists = await _context.GoalCategories
                    .AnyAsync(c => c.CategoryId == goal.CategoryId);

                if (!categoryExists)
                    return (false, "Указанная категория не существует");

                goal.CreatedAt = DateTime.UtcNow;
                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Создана новая цель: {GoalTitle}", goal.Title);
                return (true, "Цель успешно создана");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка базы данных при создании цели");
                return (false, "Ошибка сохранения в базу данных");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при создании цели");
                return (false, "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<(bool success, string message)> UpdateGoalAsync(Goal goal)
        {
            try
            {
                var existingGoal = await _context.Goals
                    .FirstOrDefaultAsync(g => g.GoalId == goal.GoalId);

                if (existingGoal == null)
                    return (false, "Цель не найдена");

                // Валидация бизнес-правил
                var validationResult = ValidateGoal(goal);
                if (!validationResult.success)
                    return validationResult;

                // Обновление полей
                existingGoal.Title = goal.Title;
                existingGoal.TargetAmount = goal.TargetAmount;
                existingGoal.StartDate = goal.StartDate;
                existingGoal.EndDate = goal.EndDate;
                existingGoal.Description = goal.Description;
                existingGoal.Priority = goal.Priority;
                existingGoal.CategoryId = goal.CategoryId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Обновлена цель: {GoalId}", goal.GoalId);
                return (true, "Цель успешно обновлена");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка базы данных при обновлении цели {GoalId}", goal.GoalId);
                return (false, "Ошибка обновления в базе данных");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при обновлении цели {GoalId}", goal.GoalId);
                return (false, "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<(bool success, string message)> DeleteGoalAsync(int goalId)
        {
            try
            {
                var goal = await _context.Goals
                    .FirstOrDefaultAsync(g => g.GoalId == goalId);

                if (goal == null)
                    return (false, "Цель не найдена");

                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Удалена цель: {GoalId}", goalId);
                return (true, "Цель успешно удалена");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка базы данных при удалении цели {GoalId}", goalId);
                return (false, "Ошибка удаления из базы данных");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при удалении цели {GoalId}", goalId);
                return (false, "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<List<Goal>> GetGoalsByCategoryAsync(int categoryId)
        {
            try
            {
                return await _context.Goals
                    .Include(g => g.GoalCategory)
                    .Include(g => g.Deposits)
                    .Where(g => g.CategoryId == categoryId)
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении целей по категории {CategoryId}", categoryId);
                return new List<Goal>();
            }
        }

        /// <summary>
        /// Валидация бизнес-правил для цели
        /// </summary>
        private (bool success, string message) ValidateGoal(Goal goal)
        {
            if (string.IsNullOrWhiteSpace(goal.Title))
                return (false, "Название цели не может быть пустым");

            if (goal.TargetAmount <= 0)
                return (false, "Целевая сумма должна быть больше 0");

            if (goal.EndDate <= goal.StartDate)
                return (false, "Дата окончания должна быть позже даты начала");

            if (goal.Priority < 1 || goal.Priority > 3)
                return (false, "Приоритет должен быть в диапазоне от 1 до 3");

            return (true, string.Empty);
        }
    }
}