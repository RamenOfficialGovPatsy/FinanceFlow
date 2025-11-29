using FinanceFlow.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Сервис для работы с целями - основной источник данных
        private readonly IGoalService _goalService;

        // Коллекция целей для отображения в UI
        private ObservableCollection<GoalViewModel> _goals = new ObservableCollection<GoalViewModel>();

        // Публичное свойство для привязки к списку целей в UI
        public ObservableCollection<GoalViewModel> Goals
        {
            get => _goals;
            set => SetProperty(ref _goals, value);
        }

        // Команды для взаимодействия с пользовательским интерфейсом
        public ICommand LoadGoalsCommand { get; } // Загрузка списка целей
        public ICommand DeleteGoalCommand { get; } // Удаление цели через контекстное меню

        // Основной конструктор с внедрением зависимостей
        public MainWindowViewModel(IGoalService goalService)
        {
            // Проверяем что сервис передан корректно
            _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));

            // Инициализируем команды с реальной логикой
            LoadGoalsCommand = new AsyncRelayCommand(LoadGoalsAsync);
            DeleteGoalCommand = new AsyncRelayCommand<GoalViewModel>(DeleteGoalAsync);

            // Загружаем цели сразу при создании ViewModel
            _ = LoadGoalsAsync();
        }

        // Основной метод загрузки целей из базы данных
        public async Task LoadGoalsAsync()
        {
            // Защита от вызова без инициализированного сервиса
            if (_goalService == null) return;

            // Получаем список целей из сервиса
            var goalsFromDb = await _goalService.GetAllGoalsAsync();

            // Очищаем текущий список и заполняем новыми данными
            Goals.Clear();
            foreach (var goal in goalsFromDb)
            {
                // Каждую модель Goal оборачиваем в GoalViewModel
                // Это позволяет добавить UI-логику и привязки для отображения
                Goals.Add(new GoalViewModel(goal));
            }
        }

        // Удаление цели по команде из контекстного меню
        private async Task DeleteGoalAsync(GoalViewModel? goalVm)
        {
            // Проверяем что передан корректный объект
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
                // Лог ошибки
                Console.WriteLine($"Ошибка удаления: {result.message}");
            }
        }
    }
}