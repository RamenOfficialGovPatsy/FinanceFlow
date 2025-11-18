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
        private ObservableCollection<GoalViewModel> _goals = new ObservableCollection<GoalViewModel>();

        public ObservableCollection<GoalViewModel> Goals
        {
            get => _goals;
            set => SetProperty(ref _goals, value);
        }

        public ICommand LoadGoalsCommand { get; }

        public MainWindowViewModel(IGoalService? goalService)
        {
            _goalService = goalService;
            LoadGoalsCommand = new AsyncRelayCommand(LoadGoalsAsync);

            InitializeTestData();
            // Временные тестовые данные
            //  Goals.Add(new Goal { GoalId = 1, Title = "Тестовый iPhone", CurrentAmount = 45000, TargetAmount = 120000 });
            //  Goals.Add(new Goal { GoalId = 2, Title = "Тестовая поездка", CurrentAmount = 25000, TargetAmount = 80000 });
        }

        private void InitializeTestData()
        {
            var testGoal1 = new Goal
            {
                GoalId = 1,
                Title = "IPHONE 15 PRO",
                CurrentAmount = 45000,
                TargetAmount = 120000,
                CategoryId = 1,
                Priority = 2,
                StartDate = DateTime.Now.AddDays(-45),
                EndDate = DateTime.Now.AddDays(45)
            };

            var testGoal2 = new Goal
            {
                GoalId = 2,
                Title = "ПОЕЗДКА В ЯПОНИЮ",
                CurrentAmount = 62000,
                TargetAmount = 200000,
                CategoryId = 3,
                Priority = 3,
                StartDate = DateTime.Now.AddDays(-120),
                EndDate = DateTime.Now.AddDays(120)
            };

            Goals.Add(new GoalViewModel(testGoal1));
            Goals.Add(new GoalViewModel(testGoal2));
        }

        private async Task LoadGoalsAsync()
        {
            await Task.Delay(100);
        }
    }
}