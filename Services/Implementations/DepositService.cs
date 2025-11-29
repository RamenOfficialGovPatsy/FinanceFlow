using FinanceFlow.Data;
using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceFlow.Services.Implementations
{
    public class DepositService : IDepositService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DepositService> _logger;

        public DepositService(AppDbContext context, ILogger<DepositService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Получение всех пополнений с информацией о целях
        public async Task<List<GoalDeposit>> GetAllDepositsAsync()
        {
            return await _context.GoalDeposits
                .Include(d => d.Goal)
                .OrderByDescending(d => d.DepositDate)
                .ToListAsync();
        }

        // Получение пополнений для конкретной цели
        public async Task<List<GoalDeposit>> GetDepositsByGoalAsync(int goalId)
        {
            return await _context.GoalDeposits
                .Include(d => d.Goal)
                .Where(d => d.GoalId == goalId)
                .OrderByDescending(d => d.DepositDate)
                .ToListAsync();
        }

        // Добавление нового пополнения с транзакцией
        public async Task<(bool success, string message)> AddDepositAsync(GoalDeposit deposit)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (deposit.Amount <= 0) return (false, "Сумма должна быть больше 0");

                var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == deposit.GoalId);
                if (goal == null) return (false, "Цель не найдена");

                // Нормализация даты для Npgsql
                deposit.DepositDate = DateTime.SpecifyKind(deposit.DepositDate, DateTimeKind.Unspecified);

                _context.GoalDeposits.Add(deposit);

                // Обновляем текущую сумму цели и проверяем завершение
                goal.CurrentAmount += deposit.Amount;
                CheckCompletion(goal);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Пополнение добавлено");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка добавления");
                return (false, "Ошибка БД");
            }
        }

        // Обновление существующего пополнения
        public async Task<(bool success, string message)> UpdateDepositAsync(GoalDeposit deposit)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (deposit.Amount <= 0) return (false, "Сумма должна быть больше 0");

                var existingDeposit = await _context.GoalDeposits
                    .Include(d => d.Goal)
                    .FirstOrDefaultAsync(d => d.DepositId == deposit.DepositId);

                if (existingDeposit == null) return (false, "Пополнение не найдено");
                var goal = existingDeposit.Goal;

                // Откатываем старую сумму и применяем новую
                goal.CurrentAmount -= existingDeposit.Amount;
                existingDeposit.Amount = deposit.Amount;
                existingDeposit.DepositType = deposit.DepositType;
                existingDeposit.Comment = deposit.Comment;
                goal.CurrentAmount += existingDeposit.Amount;

                CheckCompletion(goal);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Пополнение обновлено");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка обновления");
                return (false, "Ошибка БД");
            }
        }

        // Удаление пополнения с коррекцией суммы цели
        public async Task<(bool success, string message)> DeleteDepositAsync(int depositId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var deposit = await _context.GoalDeposits
                    .Include(d => d.Goal)
                    .FirstOrDefaultAsync(d => d.DepositId == depositId);

                if (deposit == null) return (false, "Пополнение не найдено");

                var goal = deposit.Goal;
                goal.CurrentAmount -= deposit.Amount;
                CheckCompletion(goal);

                _context.GoalDeposits.Remove(deposit);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Пополнение удалено");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка удаления");
                return (false, "Ошибка БД");
            }
        }

        // Получение общей суммы пополнений для цели
        public async Task<decimal> GetTotalDepositsByGoalAsync(int goalId)
        {
            return await _context.GoalDeposits
                .Where(d => d.GoalId == goalId)
                .SumAsync(d => d.Amount);
        }

        // Проверка и обновление статуса завершения цели
        private void CheckCompletion(Goal goal)
        {
            if (goal.CurrentAmount < 0) goal.CurrentAmount = 0;

            if (goal.CurrentAmount >= goal.TargetAmount)
            {
                goal.CurrentAmount = goal.TargetAmount;
                goal.IsCompleted = true;
            }
            else
            {
                goal.IsCompleted = false;
            }
        }
    }
}