using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class DepositViewModel : ViewModelBase
    {
        private Goal _goal;
        private readonly IDepositService _depositService;
        private readonly IGoalService _goalService;

        // Состояние редактирования
        private bool _isEditMode;
        private int _editingDepositId;

        public event Action? OnProgressUpdated;
        public event Action? RequestClose;

        // --- Свойства ввода ---

        private decimal? _amount = 1000;
        public decimal? Amount
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

        // --- Свойства состояния UI ---

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    OnPropertyChanged(nameof(ButtonText));
                    OnPropertyChanged(nameof(ButtonIcon));
                }
            }
        }

        public string ButtonText => IsEditMode ? "Сохранить" : "Внести средства";
        public string ButtonIcon => IsEditMode ? "💾" : "💰";

        public ObservableCollection<string> DepositTypes { get; } = new()
        {
            "Обычное", "Зарплата", "Фриланс", "Бонус", "Другое"
        };

        // --- Свойства цели ---

        public string GoalTitle => _goal.Title;
        public decimal CurrentAmount => _goal.CurrentAmount;
        public string ProgressText => $"{CurrentAmount:N0} / {_goal.TargetAmount:N0} ₽";

        public string ProgressPercent
        {
            get
            {
                if (_goal.TargetAmount == 0) return "(0%)";
                var percent = (CurrentAmount / _goal.TargetAmount) * 100;
                return $"({Math.Min(percent, 100):F0}%)";
            }
        }

        public ObservableCollection<DepositItemViewModel> DepositHistory { get; } = new();

        // --- Команды ---

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteHistoryItemCommand { get; }
        public ICommand StartEditCommand { get; }
        public ICommand CancelEditCommand { get; }

        // --- Конструкторы ---

        public DepositViewModel()
        {
            _goal = new Goal { Title = "Design Goal", TargetAmount = 100000 };
            _depositService = null!;
            _goalService = null!;

            SaveCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            CancelCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            DeleteHistoryItemCommand = new AsyncRelayCommand<DepositItemViewModel>(_ => Task.CompletedTask);
            StartEditCommand = new AsyncRelayCommand<DepositItemViewModel>(_ => Task.CompletedTask);
            CancelEditCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        }

        public DepositViewModel(Goal goal, IDepositService depositService, IGoalService goalService)
        {
            _goal = goal ?? throw new ArgumentNullException(nameof(goal));
            _depositService = depositService ?? throw new ArgumentNullException(nameof(depositService));
            _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeleteHistoryItemCommand = new AsyncRelayCommand<DepositItemViewModel>(DeleteDepositAsync);

            // Команда начала редактирования
            StartEditCommand = new AsyncRelayCommand<DepositItemViewModel>(StartEdit);

            CancelCommand = new AsyncRelayCommand(() =>
            {
                RequestClose?.Invoke();
                return Task.CompletedTask;
            });

            // Команда отмены редактирования (сброс формы)
            CancelEditCommand = new AsyncRelayCommand(() =>
            {
                ResetForm();
                return Task.CompletedTask;
            });

            _ = LoadHistoryAsync();
        }

        // --- Логика ---

        private async Task LoadHistoryAsync()
        {
            if (_depositService == null) return;
            var deposits = await _depositService.GetDepositsByGoalAsync(_goal.GoalId);
            DepositHistory.Clear();
            foreach (var dep in deposits) DepositHistory.Add(new DepositItemViewModel(dep));
        }

        private async Task ReloadGoalFromDb()
        {
            if (_goalService == null) return;
            var updatedGoal = await _goalService.GetGoalByIdAsync(_goal.GoalId);
            if (updatedGoal != null)
            {
                _goal.CurrentAmount = updatedGoal.CurrentAmount;
                _goal.IsCompleted = updatedGoal.IsCompleted;
                OnPropertyChanged(nameof(CurrentAmount));
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }

        // FIX: Изменили void на Task
        private Task StartEdit(DepositItemViewModel? item)
        {
            if (item == null) return Task.CompletedTask;

            _editingDepositId = item.DepositId;
            Amount = item.Amount;
            Comment = item.Comment;
            SelectedDepositType = ConvertKeyToType(item.TypeKey);

            IsEditMode = true;

            return Task.CompletedTask;
        }

        private void ResetForm()
        {
            Amount = 1000;
            Comment = string.Empty;
            SelectedDepositType = "Обычное";
            IsEditMode = false;
            _editingDepositId = 0;
        }

        private async Task SaveAsync()
        {
            decimal valueToSave = Amount ?? 0;

            // 1. Валидация
            if (valueToSave <= 0)
            {
                ShowError("Сумма пополнения должна быть больше 0.");
                return;
            }

            try
            {
                var deposit = new GoalDeposit
                {
                    GoalId = _goal.GoalId,
                    Amount = valueToSave,
                    DepositType = ConvertTypeToKey(SelectedDepositType),
                    Comment = Comment,
                    DepositDate = DateTime.Now
                };

                bool success;
                string message;

                if (IsEditMode)
                {
                    deposit.DepositId = _editingDepositId;
                    (success, message) = await _depositService.UpdateDepositAsync(deposit);
                }
                else
                {
                    (success, message) = await _depositService.AddDepositAsync(deposit);
                }

                if (success)
                {
                    await ReloadGoalFromDb();
                    await LoadHistoryAsync();
                    OnProgressUpdated?.Invoke();

                    if (IsEditMode)
                    {
                        ResetForm();
                        ShowSuccess("Запись успешно обновлена."); // Опционально, можно просто молча
                    }
                    else
                    {
                        RequestClose?.Invoke();
                        // ShowSuccess("Пополнение успешно добавлено."); // Обычно окно просто закрывается
                    }
                }
                else
                {
                    // Ошибка от сервиса (логики)
                    ShowError($"Не удалось выполнить операцию: {message}");
                }
            }
            catch (Exception ex)
            {
                // Критическая ошибка (например, БД отключилась)
                ShowError($"Критическая ошибка: {ex.Message}");
            }
        }

        private async Task DeleteDepositAsync(DepositItemViewModel? itemVm)
        {
            if (itemVm == null) return;

            if (IsEditMode && itemVm.DepositId == _editingDepositId)
            {
                ResetForm();
            }

            var result = await _depositService.DeleteDepositAsync(itemVm.DepositId);

            if (result.success)
            {
                await ReloadGoalFromDb();
                DepositHistory.Remove(itemVm);
                OnProgressUpdated?.Invoke();
                Console.WriteLine("Пополнение удалено");
            }
            else
            {
                Console.WriteLine($"Ошибка удаления: {result.message}");
            }
        }

        private string ConvertTypeToKey(string displayType)
        {
            return displayType switch
            {
                "Обычное" => "regular",
                "Зарплата" => "salary",
                "Фриланс" => "freelance",
                "Бонус" => "bonus",
                _ => "other"
            };
        }

        private string ConvertKeyToType(string key)
        {
            return key switch
            {
                "salary" => "Зарплата",
                "freelance" => "Фриланс",
                "bonus" => "Бонус",
                "other" => "Другое",
                _ => "Обычное"
            };
        }
    }

    public class DepositItemViewModel
    {
        public int DepositId { get; }
        public decimal Amount { get; }
        public DateTime Date { get; }
        public string TypeKey { get; }
        public string Comment { get; }

        public DepositItemViewModel(GoalDeposit deposit)
        {
            DepositId = deposit.DepositId;
            Amount = deposit.Amount;
            Date = deposit.DepositDate;
            TypeKey = deposit.DepositType;
            Comment = deposit.Comment ?? string.Empty;
        }

        public string DisplayType => TypeKey switch
        {
            "salary" => "Зарплата",
            "freelance" => "Фриланс",
            "bonus" => "Бонус",
            "other" => "Другое",
            _ => "Обычное"
        };

        public string Icon => TypeKey switch
        {
            "salary" => "🔹",
            "freelance" => "🔸",
            "bonus" => "❇️",
            "other" => "▫️",
            _ => "🔹"
        };

        public string IconColor => TypeKey switch
        {
            "salary" => "#3B82F6",
            "freelance" => "#F59E0B",
            "bonus" => "#10B981",
            "other" => "#9CA3AF",
            _ => "#8B5CF6"
        };

        public bool HasComment => !string.IsNullOrWhiteSpace(Comment);
    }
}