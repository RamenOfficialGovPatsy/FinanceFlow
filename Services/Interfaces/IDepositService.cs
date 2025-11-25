using FinanceFlow.Models;

namespace FinanceFlow.Services.Interfaces
{
    public interface IDepositService
    {
        Task<List<GoalDeposit>> GetAllDepositsAsync();
        Task<List<GoalDeposit>> GetDepositsByGoalAsync(int goalId);
        Task<(bool success, string message)> AddDepositAsync(GoalDeposit deposit);
        Task<(bool success, string message)> UpdateDepositAsync(GoalDeposit deposit);
        Task<(bool success, string message)> DeleteDepositAsync(int depositId);
        Task<decimal> GetTotalDepositsByGoalAsync(int goalId);
    }
}