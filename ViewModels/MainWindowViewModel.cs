using FinanceFlow.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IGoalService _goalService;
        private ObservableCollection<GoalViewModel> _goals = new ObservableCollection<GoalViewModel>();

        // Коллекция целей для отображения в UI
        public ObservableCollection<GoalViewModel> Goals
        {
            get => _goals;
            set => SetProperty(ref _goals, value);
        }

        // Команды
        public ICommand LoadGoalsCommand { get; }
        public ICommand DeleteGoalCommand { get; } // Команда для контекстного меню

        // Конструктор
        public MainWindowViewModel(IGoalService goalService)
        {
            _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));

            LoadGoalsCommand = new AsyncRelayCommand(LoadGoalsAsync);
            DeleteGoalCommand = new AsyncRelayCommand<GoalViewModel>(DeleteGoalAsync);

            // При старте сразу загружаем данные
            _ = LoadGoalsAsync();
        }

        // Загрузка данных из БД
        public async Task LoadGoalsAsync()
        {
            if (_goalService == null) return;

            // Получаем список целей из сервиса
            var goalsFromDb = await _goalService.GetAllGoalsAsync();

            // Очищаем текущий список и заполняем новыми данными
            Goals.Clear();
            foreach (var goal in goalsFromDb)
            {
                // Оборачиваем модель (Goal) во ViewModel (GoalViewModel) для удобства UI
                Goals.Add(new GoalViewModel(goal));
            }
        }

        // Удаление цели
        private async Task DeleteGoalAsync(GoalViewModel? goalVm)
        {
            if (goalVm == null) return;

            // Вызываем сервис для удаления из БД
            var result = await _goalService.DeleteGoalAsync(goalVm.GoalId);

            if (result.success)
            {
                // Если удаление в БД прошло успешно, убираем из списка на экране
                Goals.Remove(goalVm);
            }
            else
            {
                Console.WriteLine($"Ошибка удаления: {result.message}");
            }
        }
    }
}