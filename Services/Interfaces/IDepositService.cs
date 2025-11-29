using FinanceFlow.Models;

namespace FinanceFlow.Services.Interfaces
{
    public interface IDepositService
    {
        // Получение всех пополнений из базы данных
        Task<List<GoalDeposit>> GetAllDepositsAsync();

        // Получение пополнений для конкретной цели по ID
        Task<List<GoalDeposit>> GetDepositsByGoalAsync(int goalId);

        // Добавление нового пополнения с проверкой валидации
        Task<(bool success, string message)> AddDepositAsync(GoalDeposit deposit);

        // Обновление существующего пополнения
        Task<(bool success, string message)> UpdateDepositAsync(GoalDeposit deposit);

        // Удаление пополнения по ID
        Task<(bool success, string message)> DeleteDepositAsync(int depositId);

        // Получение общей суммы пополнений для цели
        Task<decimal> GetTotalDepositsByGoalAsync(int goalId);
    }
}