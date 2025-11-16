using FinanceFlow.Data;
using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceFlow.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для управления пополнениями
    /// </summary>
    public class DepositService : IDepositService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DepositService> _logger;

        public DepositService(AppDbContext context, ILogger<DepositService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<List<GoalDeposit>> GetAllDepositsAsync()
        {
            try
            {
                return await _context.GoalDeposits
                    .Include(d => d.Goal)
                    .ThenInclude(g => g!.GoalCategory)
                    .OrderByDescending(d => d.DepositDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пополнений");
                return new List<GoalDeposit>();
            }
        }

        /// <inheritdoc/>
        public async Task<List<GoalDeposit>> GetDepositsByGoalAsync(int goalId)
        {
            try
            {
                return await _context.GoalDeposits
                    .Include(d => d.Goal)
                    .Where(d => d.GoalId == goalId)
                    .OrderByDescending(d => d.DepositDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пополнений по цели {GoalId}", goalId);
                return new List<GoalDeposit>();
            }
        }

        /// <inheritdoc/>
        public async Task<(bool success, string message)> AddDepositAsync(GoalDeposit deposit)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Валидация бизнес-правил
                var validationResult = await ValidateDepositAsync(deposit);
                if (!validationResult.success)
                    return validationResult;

                // Получаем цель с блокировкой для предотвращения гонок
                var goal = await _context.Goals
                    .FirstOrDefaultAsync(g => g.GoalId == deposit.GoalId);

                if (goal == null)
                    return (false, "Цель не найдена");

                // Добавляем пополнение
                deposit.DepositDate = DateTime.UtcNow;
                _context.GoalDeposits.Add(deposit);

                // Обновляем сумму цели
                goal.CurrentAmount += deposit.Amount;

                // Проверяем выполнение цели
                if (goal.CurrentAmount >= goal.TargetAmount)
                {
                    goal.IsCompleted = true;
                    goal.CurrentAmount = goal.TargetAmount; // Не даем превысить целевую сумму
                }

                // Сохраняем изменения в рамках одной транзакции
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Добавлено пополнение {Amount} для цели {GoalId}",
                    deposit.Amount, deposit.GoalId);

                return (true, "Пополнение успешно добавлено");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Ошибка базы данных при добавлении пополнения");
                return (false, "Ошибка сохранения в базу данных");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Неожиданная ошибка при добавлении пополнения");
                return (false, "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<(bool success, string message)> DeleteDepositAsync(int depositId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var deposit = await _context.GoalDeposits
                    .Include(d => d.Goal)
                    .FirstOrDefaultAsync(d => d.DepositId == depositId);

                if (deposit == null)
                    return (false, "Пополнение не найдено");

                var goal = deposit.Goal;
                if (goal != null)
                {
                    // Возвращаем сумму из цели
                    goal.CurrentAmount -= deposit.Amount;

                    // Если сумма стала меньше целевой, снимаем отметку о выполнении
                    if (goal.CurrentAmount < goal.TargetAmount)
                    {
                        goal.IsCompleted = false;
                    }

                    // Не даем уйти в отрицательные значения
                    if (goal.CurrentAmount < 0)
                        goal.CurrentAmount = 0;
                }

                _context.GoalDeposits.Remove(deposit);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Удалено пополнение {DepositId}", depositId);
                return (true, "Пополнение успешно удалено");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Ошибка базы данных при удалении пополнения {DepositId}", depositId);
                return (false, "Ошибка удаления из базы данных");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Неожиданная ошибка при удалении пополнения {DepositId}", depositId);
                return (false, "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<decimal> GetTotalDepositsByGoalAsync(int goalId)
        {
            try
            {
                return await _context.GoalDeposits
                    .Where(d => d.GoalId == goalId)
                    .SumAsync(d => d.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при расчете общей суммы пополнений для цели {GoalId}", goalId);
                return 0;
            }
        }

        /// <summary>
        /// Валидация бизнес-правил для пополнения
        /// </summary>
        private async Task<(bool success, string message)> ValidateDepositAsync(GoalDeposit deposit)
        {
            if (deposit.Amount <= 0)
                return (false, "Сумма пополнения должна быть больше 0");

            // Проверяем существование цели
            var goal = await _context.Goals
                .FirstOrDefaultAsync(g => g.GoalId == deposit.GoalId);

            if (goal == null)
                return (false, "Цель не найдена");

            // Нельзя пополнять завершенную цель
            if (goal.IsCompleted)
                return (false, "Нельзя пополнять завершенную цель");

            // Проверяем допустимые типы пополнений
            var validTypes = new[] { "regular", "salary", "freelance", "bonus", "other" };
            if (!validTypes.Contains(deposit.DepositType))
                return (false, "Недопустимый тип пополнения");

            return (true, string.Empty);
        }
    }
}