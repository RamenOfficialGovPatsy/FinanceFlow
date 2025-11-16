using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IGoalService? _goalService;
        private ObservableCollection<Goal> _goals = new ObservableCollection<Goal>();

        public ObservableCollection<Goal> Goals
        {
            get => _goals;
            set => SetProperty(ref _goals, value);
        }

        public ICommand LoadGoalsCommand { get; }

        public MainWindowViewModel(IGoalService? goalService)
        {
            _goalService = goalService;
            LoadGoalsCommand = new AsyncRelayCommand(LoadGoalsAsync);

            // Временные тестовые данные
            Goals.Add(new Goal { GoalId = 1, Title = "Тестовый iPhone", CurrentAmount = 45000, TargetAmount = 120000 });
            Goals.Add(new Goal { GoalId = 2, Title = "Тестовая поездка", CurrentAmount = 25000, TargetAmount = 80000 });
        }

        private async Task LoadGoalsAsync()
        {
            await Task.Delay(100);
        }
    }
}