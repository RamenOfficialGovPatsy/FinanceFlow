using FinanceFlow.Data;
using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<List<GoalDeposit>> GetAllDepositsAsync()
        {
            return await _context.GoalDeposits
                .Include(d => d.Goal)
                .OrderByDescending(d => d.DepositDate)
                .ToListAsync();
        }

        public async Task<List<GoalDeposit>> GetDepositsByGoalAsync(int goalId)
        {
            return await _context.GoalDeposits
                .Include(d => d.Goal)
                .Where(d => d.GoalId == goalId)
                .OrderByDescending(d => d.DepositDate)
                .ToListAsync();
        }

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

                // Обновляем цель
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

                // 1. Откатываем старую сумму
                goal.CurrentAmount -= existingDeposit.Amount;

                // 2. Применяем новые данные
                existingDeposit.Amount = deposit.Amount;
                existingDeposit.DepositType = deposit.DepositType;
                existingDeposit.Comment = deposit.Comment; // <-- ВОТ ЭТО ОБНОВЛЯЕТ КОММЕНТАРИЙ

                // (Дату обычно не меняем при редактировании суммы, но если нужно - раскомментируй)
                // existingDeposit.DepositDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                // 3. Применяем новую сумму
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

        public async Task<decimal> GetTotalDepositsByGoalAsync(int goalId)
        {
            return await _context.GoalDeposits
                .Where(d => d.GoalId == goalId)
                .SumAsync(d => d.Amount);
        }

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