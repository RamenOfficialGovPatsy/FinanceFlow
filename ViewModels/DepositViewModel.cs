using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class DepositViewModel : ViewModelBase
    {
        // Основные зависимости и состояние
        private Goal _goal; // Цель для которой управляем пополнениями
        private readonly IDepositService _depositService; // Сервис работы с пополнениями
        private readonly IGoalService _goalService; // Сервис работы с целями

        // Состояние редактирования
        private bool _isEditMode;
        private int _editingDepositId;  // ID пополнения которое редактируем

        // События для коммуникации с View
        public event Action? OnProgressUpdated; // Вызывается при обновлении прогресса цели
        public event Action? RequestClose; // Запрос на закрытие окна

        // Свойства ввода данных пополнения
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

        //  Свойства состояния UI
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    // При изменении режима обновляем текст и иконку кнопки
                    OnPropertyChanged(nameof(ButtonText));
                    OnPropertyChanged(nameof(ButtonIcon));
                }
            }
        }

        // Текст кнопки зависит от режима (редактирование или создание)
        public string ButtonText => IsEditMode ? "Сохранить" : "Внести средства";

        // Эмодзи для визуального отличия
        public string ButtonIcon => IsEditMode ? "💾" : "💰";

        // Коллекция доступных типов пополнений для выпадающего списка
        public ObservableCollection<string> DepositTypes { get; } = new()
        {
            "Обычное", "Зарплата", "Фриланс", "Бонус", "Другое"
        };

        // Свойства отображения информации о цели 

        // Название текущей цели
        public string GoalTitle => _goal.Title;

        // Текущая накопленная сумма
        public decimal CurrentAmount => _goal.CurrentAmount;

        // Прогресс в виде текста
        public string ProgressText => $"{CurrentAmount:N0} / {_goal.TargetAmount:N0} ₽";

        // Процент выполнения цели с защитой от превышения 100%
        public string ProgressPercent
        {
            get
            {
                if (_goal.TargetAmount == 0) return "(0%)";
                var percent = (CurrentAmount / _goal.TargetAmount) * 100;
                return $"({Math.Min(percent, 100):F0}%)";
            }
        }

        // Коллекция для отображения истории пополнений
        public ObservableCollection<DepositItemViewModel> DepositHistory { get; } = new();

        // Команды для взаимодействия с UI
        public ICommand SaveCommand { get; } // Сохранение пополнения
        public ICommand CancelCommand { get; } // Отмена и закрытие
        public ICommand DeleteHistoryItemCommand { get; } // Удаление из истории
        public ICommand StartEditCommand { get; } // Начало редактирования
        public ICommand CancelEditCommand { get; } // Отмена редактирования

        // Конструкторы

        // Конструктор для дизайнера
        public DepositViewModel()
        {
            _goal = new Goal { Title = "Design Goal", TargetAmount = 100000 };
            _depositService = null!;
            _goalService = null!;

            // Заглушки для команд в режиме дизайнера
            SaveCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            CancelCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            DeleteHistoryItemCommand = new AsyncRelayCommand<DepositItemViewModel>(_ => Task.CompletedTask);
            StartEditCommand = new AsyncRelayCommand<DepositItemViewModel>(_ => Task.CompletedTask);
            CancelEditCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        }

        // Основной конструктор с реальными зависимостями
        public DepositViewModel(Goal goal, IDepositService depositService, IGoalService goalService)
        {
            _goal = goal ?? throw new ArgumentNullException(nameof(goal));
            _depositService = depositService ?? throw new ArgumentNullException(nameof(depositService));
            _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));

            // Инициализация команд с реальной логикой
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
                ResetForm(); // Сбрасываем форму при отмене редактирования
                return Task.CompletedTask;
            });

            // Загружаем историю асинхронно при создании
            _ = LoadHistoryAsync();
        }

        //  Основная логика работы с пополнениями

        // Загрузка истории пополнений из базы данных
        private async Task LoadHistoryAsync()
        {
            if (_depositService == null) return;
            var deposits = await _depositService.GetDepositsByGoalAsync(_goal.GoalId);
            DepositHistory.Clear();
            foreach (var dep in deposits) DepositHistory.Add(new DepositItemViewModel(dep));
        }

        // Перезагрузка данных цели из базы для актуального прогресса
        private async Task ReloadGoalFromDb()
        {
            if (_goalService == null) return;
            var updatedGoal = await _goalService.GetGoalByIdAsync(_goal.GoalId);
            if (updatedGoal != null)
            {
                _goal.CurrentAmount = updatedGoal.CurrentAmount;
                _goal.IsCompleted = updatedGoal.IsCompleted;

                // Уведомляем об изменении свойств для обновления UI
                OnPropertyChanged(nameof(CurrentAmount));
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }

        // Начало редактирования существующего пополнения
        private Task StartEdit(DepositItemViewModel? item)
        {
            if (item == null) return Task.CompletedTask;

            // Заполняем форму данными из выбранного пополнения
            _editingDepositId = item.DepositId;
            Amount = item.Amount;
            Comment = item.Comment;
            SelectedDepositType = ConvertKeyToType(item.TypeKey);

            // Переключаемся в режим редактирования
            IsEditMode = true;

            return Task.CompletedTask;
        }

        // Сброс формы к состоянию по умолчанию
        private void ResetForm()
        {
            Amount = 1000;
            Comment = string.Empty;
            SelectedDepositType = "Обычное";
            IsEditMode = false;
            _editingDepositId = 0;
        }

        // Основной метод сохранения пополнения (создание или обновление)
        private async Task SaveAsync()
        {
            decimal valueToSave = Amount ?? 0;

            // Валидация введенной суммы
            if (valueToSave <= 0)
            {
                ShowError("Сумма пополнения должна быть больше 0.");
                return;
            }

            try
            {
                // Создаем объект пополнения с данными из формы
                var deposit = new GoalDeposit
                {
                    GoalId = _goal.GoalId,
                    Amount = valueToSave,

                    // Конвертируем в ключ для БД
                    DepositType = ConvertTypeToKey(SelectedDepositType),
                    Comment = Comment,
                    DepositDate = DateTime.Now
                };

                bool success;
                string message;

                // Выбираем метод сервиса в зависимости от режима
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
                    // Обновляем данные после успешного сохранения
                    await ReloadGoalFromDb();
                    await LoadHistoryAsync();
                    OnProgressUpdated?.Invoke(); // Уведомляем об обновлении прогресса

                    if (IsEditMode)
                    {
                        // В режиме редактирования сбрасываем форму
                        ResetForm();
                        ShowSuccess("Запись успешно обновлена."); // Опционально, можно просто молча
                    }
                    else
                    {
                        // В режиме создания закрываем окно
                        RequestClose?.Invoke();
                    }
                }
                else
                {
                    // Показываем ошибку от сервиса
                    ShowError($"Не удалось выполнить операцию: {message}");
                }
            }
            catch (Exception ex)
            {
                // Критическая ошибка (например, БД отключилась)
                ShowError($"Критическая ошибка: {ex.Message}");
            }
        }

        // Удаление пополнения из истории
        private async Task DeleteDepositAsync(DepositItemViewModel? itemVm)
        {
            if (itemVm == null) return;

            // Если удаляем редактируемый элемент - сбрасываем форму
            if (IsEditMode && itemVm.DepositId == _editingDepositId)
            {
                ResetForm();
            }

            var result = await _depositService.DeleteDepositAsync(itemVm.DepositId);

            if (result.success)
            {
                // Обновляем данные после удаления
                await ReloadGoalFromDb();
                DepositHistory.Remove(itemVm);
                OnProgressUpdated?.Invoke();
                //  Console.WriteLine("Пополнение удалено");
            }
            else
            {
                ShowError($"Не удалось удалить пополнение: {result.message}", "Ошибка удаления");
            }
        }

        // Конвертация отображаемого типа в ключ для базы данных
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

        // Конвертация ключа из базы в отображаемый тип
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

    // ViewModel для отображения элемента истории пополнений
    public class DepositItemViewModel
    {
        public int DepositId { get; } // ID пополнения
        public decimal Amount { get; } // Сумма пополнения
        public DateTime Date { get; } // Дата внесения
        public string TypeKey { get; } // Ключ типа для базы
        public string Comment { get; } // Комментарий к пополнению

        public DepositItemViewModel(GoalDeposit deposit)
        {
            DepositId = deposit.DepositId;
            Amount = deposit.Amount;
            Date = deposit.DepositDate;
            TypeKey = deposit.DepositType;
            Comment = deposit.Comment ?? string.Empty;
        }

        // Отображаемое название типа пополнения
        public string DisplayType => TypeKey switch
        {
            "salary" => "Зарплата",
            "freelance" => "Фриланс",
            "bonus" => "Бонус",
            "other" => "Другое",
            _ => "Обычное"
        };

        // Иконка для типа пополнения
        public string Icon => TypeKey switch
        {
            "salary" => "🔹",
            "freelance" => "🔸",
            "bonus" => "❇️",
            "other" => "▫️",
            _ => "🔹"
        };

        // Цвет для иконки типа пополнения
        public string IconColor => TypeKey switch
        {
            "salary" => "#3B82F6",
            "freelance" => "#F59E0B",
            "bonus" => "#10B981",
            "other" => "#9CA3AF",
            _ => "#8B5CF6"
        };

        // Флаг наличия комментария
        public bool HasComment => !string.IsNullOrWhiteSpace(Comment);
    }
}