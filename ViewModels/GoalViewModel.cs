using FinanceFlow.Models;
using Avalonia.Media.Imaging;

namespace FinanceFlow.ViewModels
{
    public class GoalViewModel : ViewModelBase
    {
        // Базовая модель цели из базы данных
        private readonly Goal _goal;

        // Локальные копии часто изменяемых свойств для оптимизации
        private decimal _currentAmount;
        private bool _isCompleted;
        private Bitmap? _goalImage; // Храним загруженную картинку

        // ФЛАГИ ЗАЩИТЫ ОТ РЕКУРСИИ (Layout Cycle Fix)
        // Предотвращают циклические обновления свойств в Avalonia
        private bool _isSettingStartDate;
        private bool _isSettingEndDate;

        // Основной конструктор - инициализирует ViewModel на основе модели Goal
        public GoalViewModel(Goal goal)
        {
            _goal = goal ?? throw new ArgumentNullException(nameof(goal));
            _currentAmount = goal.CurrentAmount;
            _isCompleted = goal.IsCompleted;

            // При создании сразу пробую загрузить картинку
            LoadImage();
        }

        // ОСНОВНЫЕ СВОЙСТВА ЦЕЛИ

        // Идентификатор цели из базы данных
        public int GoalId => _goal.GoalId;

        // Название цели с уведомлением об изменени
        public string Title
        {
            get => _goal.Title;
            set
            {
                if (_goal.Title != value)
                {
                    _goal.Title = value;
                    OnPropertyChanged();

                    // Обновляем отображаемое название
                    OnPropertyChanged(nameof(DisplayTitle));
                }
            }
        }

        // Отображаемое название с иконкой категории
        public string DisplayTitle => $"{CategoryIcon} {Title}";

        // СВОЙСТВА КАТЕГОРИИ
        public int CategoryId => _goal.CategoryId;
        public string CategoryName => _goal.GoalCategory?.Name ?? "БЕЗ КАТЕГОРИИ";
        public string CategoryIcon => _goal.GoalCategory?.Icon ?? "⭐";
        public string CategoryColor => _goal.GoalCategory?.Color ?? "#6B7280";

        // ФИНАНСОВЫЕ СВОЙСТВА

        // Текущая накопленная сумма с логикой обновления
        public decimal CurrentAmount
        {
            get => _currentAmount;
            set
            {
                if (SetProperty(ref _currentAmount, value))
                {
                    // Синхронизируем с моделью
                    _goal.CurrentAmount = value;

                    // Обновляю все зависимые свойства прогресса
                    OnPropertyChanged(nameof(RemainingAmount));
                    OnPropertyChanged(nameof(ProgressPercentage));
                    OnPropertyChanged(nameof(ProgressWidth));
                    OnPropertyChanged(nameof(ProgressColor));
                    UpdateCompletionStatus(); // Проверяем не выполнена ли цель
                }
            }
        }

        // Целевая сумма для накопления
        public decimal TargetAmount
        {
            get => _goal.TargetAmount;
            set
            {
                if (_goal.TargetAmount != value)
                {
                    _goal.TargetAmount = value;
                    OnPropertyChanged();

                    // Обновляю свойства зависящие от целевой суммы
                    OnPropertyChanged(nameof(RemainingAmount));
                    OnPropertyChanged(nameof(ProgressPercentage));
                    OnPropertyChanged(nameof(ProgressWidth));
                    UpdateCompletionStatus();
                }
            }
        }

        // Оставшаяся сумма до цели
        public decimal RemainingAmount => TargetAmount - CurrentAmount;

        // Процент выполнения цели от 0 до 100
        public decimal ProgressPercentage =>
            TargetAmount > 0 ? Math.Round((CurrentAmount / TargetAmount) * 100, 1) : 0;

        // Ширина прогресс-бара в пикселях (для визуализации)
        public double ProgressWidth =>
            Math.Min((double)ProgressPercentage, 100) * 3.0;

        // Цвет прогресс-бара в зависимости от процента выполнения
        public string ProgressColor
        {
            get
            {
                return ProgressPercentage switch
                {
                    >= 100 => "#10B981",
                    >= 75 => "#10B981",
                    >= 50 => "#F59E0B",
                    >= 25 => "#F59E0B",
                    _ => "#EF4444"
                };
            }
        }

        // СВОЙСТВА ДАТ С ЗАЩИТОЙ ОТ РЕКУРСИИ

        // Дата начала цели с защитой от циклических обновлений
        public DateTime StartDate
        {
            get => _goal.StartDate;
            set
            {
                // Защита от рекурсии и лишних обновлений
                if (_isSettingStartDate || _goal.StartDate == value) return;

                try
                {
                    _isSettingStartDate = true; // Блокируем повторный вход
                    _goal.StartDate = value;

                    OnPropertyChanged(); // Уведомляем об изменении даты

                    // Массовое обновление всех зависимых свойств дат
                    OnMultiplePropertiesChanged(
                        nameof(DaysPassed),
                        nameof(TotalDays),
                        nameof(DaysLeft),
                        nameof(DaysLeftText),
                        nameof(DaysLeftColor),
                        nameof(IsOverdue)
                    );
                }
                finally
                {
                    _isSettingStartDate = false; // Снимаем блокировку
                }
            }
        }

        // Дата окончания цели с аналогичной защитой
        public DateTime EndDate
        {
            get => _goal.EndDate;
            set
            {
                if (_isSettingEndDate || _goal.EndDate == value) return;

                try
                {
                    _isSettingEndDate = true; // Блокирую повторный вход
                    _goal.EndDate = value;

                    // Уведомляем об изменении самой даты
                    OnPropertyChanged();

                    // Обновляем свойства связанные с дедлайном
                    OnMultiplePropertiesChanged(
                        nameof(DaysLeft),
                        nameof(DaysLeftText),
                        nameof(DaysLeftColor),
                        nameof(TotalDays),
                        nameof(IsOverdue),
                        nameof(TimeProgressPercentage)
                    );
                }
                finally
                {
                    _isSettingEndDate = false; // Снимаю блокировку
                }
            }
        }

        // Вычисляемые свойства для работы с датами
        public int DaysPassed => (DateTime.Today - StartDate).Days;
        public int TotalDays => (EndDate - StartDate).Days;
        public int DaysLeft => (EndDate - DateTime.Today).Days;
        public bool IsOverdue => DaysLeft < 0 && !IsCompleted;

        // Текстовое представление оставшегося времени
        public string DaysLeftText
        {
            get
            {
                if (EndDate == DateTime.MinValue) return "";

                var today = DateTime.Today;
                var end = EndDate.Date;
                var diff = (end - today).Days;

                if (diff < 0) return $"Просрочено ({Math.Abs(diff)} дн.)";
                if (diff == 0) return "Сегодня";
                return $"{diff} дней";
            }
        }

        // Цвет индикатора времени в зависимости от срочности
        public string DaysLeftColor
        {
            get
            {
                var today = DateTime.Today;
                var end = EndDate.Date;
                var diff = (end - today).Days;

                if (diff < 0) return "#EF4444";
                if (diff <= 7) return "#EF4444";
                if (diff <= 30) return "#F59E0B";
                return "#10B981";
            }
        }

        // Процент пройденного времени от начала до конца цели
        public double TimeProgressPercentage =>
            TotalDays > 0 ? Math.Round((DaysPassed / (double)TotalDays) * 100, 1) : 0;

        // СВОЙСТВА ПРИОРИТЕТА
        public int Priority
        {
            get => _goal.Priority;
            set
            {
                if (_goal.Priority != value)
                {
                    _goal.Priority = value;
                    OnPropertyChanged();

                    // Обновляю визуальные свойства приоритета
                    OnPropertyChanged(nameof(PriorityColor));
                    OnPropertyChanged(nameof(PriorityName));
                    OnPropertyChanged(nameof(PriorityIcon));
                }
            }
        }

        // Цвет приоритета для индикации в UI
        public string PriorityColor => Priority switch
        {
            1 => "#EF4444",
            2 => "#F59E0B",
            3 => "#10B981",
            _ => "#6B7280"
        };

        public string PriorityName => Priority switch
        {
            1 => "Высокий",
            2 => "Средний",
            3 => "Низкий",
            _ => "Не указан"
        };

        public string PriorityIcon => Priority switch
        {
            1 => "🔴",
            2 => "🟡",
            3 => "🟢",
            _ => "⚪"
        };

        // СВОЙСТВА СТАТУСА ВЫПОЛНЕНИЯ
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    _goal.IsCompleted = value;
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public string StatusText => IsCompleted ? "Выполнено" : IsOverdue ? "Просрочено" : "В процессе";
        public string StatusColor => IsCompleted ? "#10B981" : IsOverdue ? "#EF4444" : "#F59E0B";

        // СВОЙСТВА ОПИСАНИЯ
        public string Description
        {
            get => _goal.Description ?? string.Empty;
            set
            {
                if (_goal.Description != value)
                {
                    _goal.Description = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasDescription));
                }
            }
        }

        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        // РАБОТА С ИЗОБРАЖЕНИЯМИ
        public string ImagePath
        {
            get => _goal.ImagePath ?? string.Empty;
            set
            {
                if (_goal.ImagePath != value)
                {
                    _goal.ImagePath = value;
                    OnPropertyChanged();
                    LoadImage(); // Перезагружаю изображение при изменении пути
                }
            }
        }

        public Bitmap? GoalImage
        {
            get => _goalImage;
            private set
            {
                if (SetProperty(ref _goalImage, value))
                {
                    OnPropertyChanged(nameof(HasImage));
                }
            }
        }

        public bool HasImage => GoalImage != null;

        // Загрузка изображения цели с обработкой ошибок
        private void LoadImage()
        {
            try
            {
                if (string.IsNullOrEmpty(_goal.ImagePath) || !File.Exists(_goal.ImagePath))
                {
                    GoalImage = null;
                    return;
                }
                using (var stream = File.OpenRead(_goal.ImagePath))
                {
                    GoalImage = new Bitmap(stream);
                }
            }
            catch (Exception)
            {
                // В случае ошибки сбрасываем изображение
                GoalImage = null;
            }
        }

        public DateTime CreatedAt => _goal.CreatedAt;

        // БИЗНЕС-МЕТОДЫ ДЛЯ РАБОТЫ С ЦЕЛЬЮ

        // Добавление суммы к текущим накоплениям
        public void AddDeposit(decimal amount)
        {
            if (amount <= 0) return;
            CurrentAmount += amount;
            if (CurrentAmount >= TargetAmount)
            {
                CurrentAmount = TargetAmount;
                IsCompleted = true;
            }
        }

        // Изъятие суммы из накоплений
        public void WithdrawDeposit(decimal amount)
        {
            if (amount <= 0 || amount > CurrentAmount) return;
            CurrentAmount -= amount;
            if (IsCompleted && CurrentAmount < TargetAmount)
            {
                IsCompleted = false;
            }
        }

        // Прямое обновление прогресса с проверками
        public void UpdateProgress(decimal newAmount)
        {
            CurrentAmount = Math.Max(0, Math.Min(newAmount, TargetAmount));
        }

        // Продление дедлайна на указанное количество дней
        public void ExtendDeadline(int additionalDays)
        {
            EndDate = EndDate.AddDays(additionalDays);
        }

        // Отметка цели как выполненной
        public void MarkAsCompleted()
        {
            IsCompleted = true;
            CurrentAmount = TargetAmount;
        }

        // Снятие отметки о выполнении
        public void MarkAsIncomplete()
        {
            IsCompleted = false;
        }

        // Валидация данных цели перед сохранением
        public (bool isValid, string errorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(Title)) return (false, "Название цели не может быть пустым");
            if (TargetAmount <= 0) return (false, "Целевая сумма должна быть больше 0");
            if (Priority < 1 || Priority > 3) return (false, "Приоритет должен быть в диапазоне от 1 до 3");
            return (true, string.Empty);
        }

        // Получение базовой модели для сохранения в БД
        public Goal GetGoalModel() => _goal;

        // Автоматическое обновление статуса выполнения при изменении сумм
        private void UpdateCompletionStatus()
        {
            if (CurrentAmount >= TargetAmount)
            {
                IsCompleted = true;
            }
        }
    }
}