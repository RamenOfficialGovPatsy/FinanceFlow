using FinanceFlow.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinanceFlow.ViewModels
{
    public class GoalViewModel : ViewModelBase
    {
        private readonly Goal _goal;
        private decimal _currentAmount;
        private bool _isCompleted;

        // Основные свойства
        public int GoalId => _goal.GoalId;

        public string Title
        {
            get => _goal.Title;
            set
            {
                if (_goal.Title != value)
                {
                    _goal.Title = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayTitle));
                }
            }
        }

        public string DisplayTitle => $"{CategoryIcon} {Title}";

        // Категория
        public int CategoryId => _goal.CategoryId;
        public string CategoryName => _goal.GoalCategory?.Name ?? "Без категории";
        public string CategoryIcon => _goal.GoalCategory?.Icon ?? "⭐";
        public string CategoryColor => _goal.GoalCategory?.Color ?? "#6B7280";

        // Финансы
        public decimal CurrentAmount
        {
            get => _currentAmount;
            set
            {
                if (SetProperty(ref _currentAmount, value))
                {
                    _goal.CurrentAmount = value;
                    OnPropertyChanged(nameof(RemainingAmount));
                    OnPropertyChanged(nameof(ProgressPercentage));
                    OnPropertyChanged(nameof(ProgressWidth));
                    OnPropertyChanged(nameof(ProgressColor));
                    UpdateCompletionStatus();
                }
            }
        }

        public decimal TargetAmount
        {
            get => _goal.TargetAmount;
            set
            {
                if (_goal.TargetAmount != value)
                {
                    _goal.TargetAmount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RemainingAmount));
                    OnPropertyChanged(nameof(ProgressPercentage));
                    OnPropertyChanged(nameof(ProgressWidth));
                    UpdateCompletionStatus();
                }
            }
        }

        public decimal RemainingAmount => TargetAmount - CurrentAmount;

        public decimal ProgressPercentage =>
            TargetAmount > 0 ? Math.Round((CurrentAmount / TargetAmount) * 100, 1) : 0;

        public double ProgressWidth =>
            Math.Min((double)ProgressPercentage, 100) * 3.0; // Для ProgressBar шириной 300px

        public string ProgressColor
        {
            get
            {
                return ProgressPercentage switch
                {
                    >= 100 => "#10B981", // Выполнено - зеленый
                    >= 75 => "#10B981",  // Близко к завершению - зеленый
                    >= 50 => "#F59E0B",  // На полпути - оранжевый
                    >= 25 => "#F59E0B",  // Начальный прогресс - оранжевый
                    _ => "#EF4444"       // Только начато - красный
                };
            }
        }

        // Даты
        public DateTime StartDate
        {
            get => _goal.StartDate;
            set
            {
                if (_goal.StartDate != value)
                {
                    _goal.StartDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DaysPassed));
                    OnPropertyChanged(nameof(TotalDays));
                    OnPropertyChanged(nameof(DaysLeft));
                    OnPropertyChanged(nameof(IsOverdue));
                }
            }
        }

        public DateTime EndDate
        {
            get => _goal.EndDate;
            set
            {
                if (_goal.EndDate != value)
                {
                    _goal.EndDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DaysLeft));
                    OnPropertyChanged(nameof(TotalDays));
                    OnPropertyChanged(nameof(IsOverdue));
                    OnPropertyChanged(nameof(TimeProgressPercentage));
                }
            }
        }

        public int DaysPassed => (DateTime.Today - StartDate).Days;
        public int TotalDays => (EndDate - StartDate).Days;
        public int DaysLeft => (EndDate - DateTime.Today).Days;
        public bool IsOverdue => DaysLeft < 0 && !IsCompleted;

        public double TimeProgressPercentage =>
            TotalDays > 0 ? Math.Round((DaysPassed / (double)TotalDays) * 100, 1) : 0;

        // Приоритет
        public int Priority
        {
            get => _goal.Priority;
            set
            {
                if (_goal.Priority != value)
                {
                    _goal.Priority = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PriorityColor));
                    OnPropertyChanged(nameof(PriorityName));
                    OnPropertyChanged(nameof(PriorityIcon));
                }
            }
        }

        public string PriorityColor => Priority switch
        {
            1 => "#EF4444", // Высокий - красный
            2 => "#F59E0B", // Средний - оранжевый  
            3 => "#10B981", // Низкий - зеленый
            _ => "#6B7280"  // По умолчанию - серый
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

        // Статус
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

        // Дополнительные свойства
        public string Description
        {
            get => _goal.Description ?? string.Empty;
            set
            {
                if (_goal.Description != value)
                {
                    _goal.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImagePath
        {
            get => _goal.ImagePath ?? string.Empty;
            set
            {
                if (_goal.ImagePath != value)
                {
                    _goal.ImagePath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasImage));
                }
            }
        }

        public bool HasImage => !string.IsNullOrEmpty(ImagePath);

        public DateTime CreatedAt => _goal.CreatedAt;

        // Конструктор
        public GoalViewModel(Goal goal)
        {
            _goal = goal ?? throw new ArgumentNullException(nameof(goal));
            _currentAmount = goal.CurrentAmount;
            _isCompleted = goal.IsCompleted;
        }

        // Бизнес-методы
        public void AddDeposit(decimal amount)
        {
            if (amount <= 0) return;

            CurrentAmount += amount;

            // Автоматически отмечаем как выполненную при достижении цели
            if (CurrentAmount >= TargetAmount)
            {
                CurrentAmount = TargetAmount; // Не даем превысить цель
                IsCompleted = true;
            }
        }

        public void WithdrawDeposit(decimal amount)
        {
            if (amount <= 0 || amount > CurrentAmount) return;

            CurrentAmount -= amount;

            // Снимаем статус выполнения если сумма ушла ниже цели
            if (IsCompleted && CurrentAmount < TargetAmount)
            {
                IsCompleted = false;
            }
        }

        public void UpdateProgress(decimal newAmount)
        {
            CurrentAmount = Math.Max(0, Math.Min(newAmount, TargetAmount));
        }

        public void ExtendDeadline(int additionalDays)
        {
            EndDate = EndDate.AddDays(additionalDays);
        }

        public void MarkAsCompleted()
        {
            IsCompleted = true;
            CurrentAmount = TargetAmount; // Гарантируем 100% прогресс
        }

        public void MarkAsIncomplete()
        {
            IsCompleted = false;
        }

        // Валидация
        public (bool isValid, string errorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return (false, "Название цели не может быть пустым");

            if (TargetAmount <= 0)
                return (false, "Целевая сумма должна быть больше 0");

            if (EndDate <= StartDate)
                return (false, "Дата окончания должна быть позже даты начала");

            if (Priority < 1 || Priority > 3)
                return (false, "Приоритет должен быть в диапазоне от 1 до 3");

            return (true, string.Empty);
        }

        // Получение исходной модели (для сохранения в БД)
        public Goal GetGoalModel() => _goal;

        private void UpdateCompletionStatus()
        {
            if (CurrentAmount >= TargetAmount)
            {
                IsCompleted = true;
            }
        }
    }
}