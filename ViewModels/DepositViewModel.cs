using FinanceFlow.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class DepositViewModel : ViewModelBase
    {
        private readonly Goal _goal;

        // --- Свойства ввода ---

        private decimal _amount = 5000;
        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _selectedDepositType = "Обычное";
        public string SelectedDepositType
        {
            get => _selectedDepositType;
            set => SetProperty(ref _selectedDepositType, value);
        }

        private string _comment = string.Empty;
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        // Список типов пополнения для ComboBox
        public ObservableCollection<string> DepositTypes { get; } = new()
        {
            "Обычное",
            "Зарплата",
            "Фриланс",
            "Бонус",
            "Другое"
        };

        // --- Свойства информации о цели (Read Only) ---

        public string GoalTitle => _goal?.Title ?? "Неизвестная цель";

        public string ProgressText
        {
            get
            {
                if (_goal == null) return "0 / 0 ₽";
                return $"{_goal.CurrentAmount:N0} / {_goal.TargetAmount:N0} ₽";
            }
        }

        public string ProgressPercent
        {
            get
            {
                if (_goal == null || _goal.TargetAmount == 0) return "(0%)";
                var percent = (_goal.CurrentAmount / _goal.TargetAmount) * 100;
                return $"({percent:F0}%)";
            }
        }

        // --- История пополнений ---

        public ObservableCollection<DepositItemViewModel> DepositHistory { get; } = new();

        // --- Команды ---

        public ICommand AddDepositCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand EditHistoryItemCommand { get; }
        public ICommand DeleteHistoryItemCommand { get; }

        // --- Конструктор ---

        // Принимает цель, для которой открыто окно
        public DepositViewModel(Goal goal)
        {
            _goal = goal ?? throw new ArgumentNullException(nameof(goal));

            // Инициализация команд
            AddDepositCommand = new AsyncRelayCommand(AddDepositAsync);
            CancelCommand = new AsyncRelayCommand(() => Task.CompletedTask); // Логику закрытия окна добавим позже
            EditHistoryItemCommand = new AsyncRelayCommand<DepositItemViewModel>(EditHistoryItemAsync);
            DeleteHistoryItemCommand = new AsyncRelayCommand<DepositItemViewModel>(DeleteHistoryItemAsync);

            // Загрузка моковых данных (как на макете)
            LoadMockHistory();
        }

        // Конструктор без параметров для Design-time (чтобы XAML не ругался)
        // Конструктор без параметров для Design-time
        public DepositViewModel()
        {
            _goal = new Goal
            {
                Title = "IPHONE 15 PRO",
                CurrentAmount = 45000,
                TargetAmount = 120000
            };

            // Инициализируем команды-заглушки, чтобы избежать Warning CS8618
            AddDepositCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            CancelCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            EditHistoryItemCommand = new AsyncRelayCommand<DepositItemViewModel>(_ => Task.CompletedTask);
            DeleteHistoryItemCommand = new AsyncRelayCommand<DepositItemViewModel>(_ => Task.CompletedTask);

            LoadMockHistory();
        }

        // --- Логика ---

        private void LoadMockHistory()
        {
            // Эти данные в будущем будут браться из БД (GoalDeposit)
            DepositHistory.Add(new DepositItemViewModel
            {
                DepositId = 1,
                Date = new DateTime(2023, 11, 25),
                Amount = 10000,
                Type = "Зарплата",
                Comment = null
            });

            DepositHistory.Add(new DepositItemViewModel
            {
                DepositId = 2,
                Date = new DateTime(2023, 11, 15),
                Amount = 15000,
                Type = "Фриланс",
                Comment = "Проект для клиента"
            });

            DepositHistory.Add(new DepositItemViewModel
            {
                DepositId = 3,
                Date = new DateTime(2023, 11, 01),
                Amount = 20000,
                Type = "Зарплата",
                Comment = "Основная зарплата"
            });
        }

        private async Task AddDepositAsync()
        {
            // Здесь будет вызов DepositService для сохранения в БД
            await Task.Delay(100);
            Console.WriteLine($"Добавляем пополнение: {Amount} ₽, Тип: {SelectedDepositType}, Коммент: {Comment}");
        }

        private async Task EditHistoryItemAsync(DepositItemViewModel? item)
        {
            if (item == null) return;
            await Task.Delay(50);
            Console.WriteLine($"Редактируем запись ID: {item.DepositId}");
        }

        private async Task DeleteHistoryItemAsync(DepositItemViewModel? item)
        {
            if (item == null) return;
            await Task.Delay(50);
            DepositHistory.Remove(item); // Удаляем визуально для теста
            Console.WriteLine($"Удаляем запись ID: {item.DepositId}");
        }
    }

    /// <summary>
    /// Вспомогательная модель для элемента списка истории.
    /// Содержит логику отображения иконки и цвета в зависимости от типа.
    /// </summary>
    public class DepositItemViewModel
    {
        public int DepositId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Comment { get; set; }

        // Вычисляемые свойства для UI (иконки и цвета)

        public string Icon => Type switch
        {
            "Зарплата" => "🔹",
            "Фриланс" => "🔸",
            "Бонус" => "🔸", // Зеленого ромба нет в стандартных эмодзи, используем оранжевый или можно "❇️"
            "Обычное" => "🔹",
            _ => "▫️"
        };

        public string IconColor => Type switch
        {
            "Зарплата" => "#3B82F6", // Синий
            "Фриланс" => "#F59E0B", // Оранжевый
            "Бонус" => "#10B981",   // Зеленый
            "Обычное" => "#8B5CF6", // Фиолетовый
            _ => "#9CA3AF"          // Серый
        };

        public bool HasComment => !string.IsNullOrEmpty(Comment);
    }

    // Вспомогательный класс для команд с параметрами (если у вас его еще нет в AsyncRelayCommand.cs)
    // Если есть - удалите этот блок.
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Func<T?, bool>? _canExecute;

        // Добавляем пустые add/remove, чтобы убрать warning "Event is never used"
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        public async void Execute(object? parameter)
        {
            await _execute((T?)parameter);
        }
    }
}