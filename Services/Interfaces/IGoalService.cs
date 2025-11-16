using FinanceFlow.Models;

namespace FinanceFlow.Services.Interfaces
{
    public interface IGoalService
    {
        Task<List<Goal>> GetAllGoalsAsync();
        Task<Goal?> GetGoalByIdAsync(int goalId);
        Task<(bool success, string message)> AddGoalAsync(Goal goal);
        Task<(bool success, string message)> UpdateGoalAsync(Goal goal);
        Task<(bool success, string message)> DeleteGoalAsync(int goalId);
        Task<List<Goal>> GetGoalsByCategoryAsync(int categoryId);
    }
}