using FinanceFlow.Models;
using Avalonia.Media.Imaging; // Для работы с картинками
using System;
using System.ComponentModel;
using System.IO; // Для проверки существования файла
using System.Runtime.CompilerServices;

namespace FinanceFlow.ViewModels
{
    public class GoalViewModel : ViewModelBase
    {
        private readonly Goal _goal;
        private decimal _currentAmount;
        private bool _isCompleted;
        private Bitmap? _goalImage; // Храним загруженную картинку

        // Конструктор
        public GoalViewModel(Goal goal)
        {
            _goal = goal ?? throw new ArgumentNullException(nameof(goal));
            _currentAmount = goal.CurrentAmount;
            _isCompleted = goal.IsCompleted;

            // При создании сразу пробуем загрузить картинку
            LoadImage();
        }

        // --- Основные свойства ---
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

        // --- Категория ---
        public int CategoryId => _goal.CategoryId;
        public string CategoryName => _goal.GoalCategory?.Name ?? "БЕЗ КАТЕГОРИИ";
        public string CategoryIcon => _goal.GoalCategory?.Icon ?? "⭐";
        public string CategoryColor => _goal.GoalCategory?.Color ?? "#6B7280";

        // --- Финансы ---
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
            Math.Min((double)ProgressPercentage, 100) * 3.0;

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

        // --- Даты ---
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
                    OnPropertyChanged(nameof(DaysLeftText));
                    OnPropertyChanged(nameof(DaysLeftColor));
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
                    OnPropertyChanged(nameof(DaysLeftText));
                    OnPropertyChanged(nameof(DaysLeftColor));
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

        public double TimeProgressPercentage =>
            TotalDays > 0 ? Math.Round((DaysPassed / (double)TotalDays) * 100, 1) : 0;

        // --- Приоритет ---
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

        // --- Статус ---
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

        // --- Описание (с логикой для UI) ---
        public string Description
        {
            get => _goal.Description ?? string.Empty;
            set
            {
                if (_goal.Description != value)
                {
                    _goal.Description = value;
                    OnPropertyChanged();
                    // Уведомляем UI, что наличие описания изменилось
                    OnPropertyChanged(nameof(HasDescription));
                }
            }
        }

        // Свойство-флаг для UI: есть ли описание?
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        // --- Изображение (с логикой загрузки) ---
        public string ImagePath
        {
            get => _goal.ImagePath ?? string.Empty;
            set
            {
                if (_goal.ImagePath != value)
                {
                    _goal.ImagePath = value;
                    OnPropertyChanged();

                    // При изменении пути пытаемся перезагрузить картинку
                    LoadImage();
                }
            }
        }

        // Bitmap для отображения в Image контроле
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

        // Свойство-флаг для UI: есть ли картинка?
        public bool HasImage => GoalImage != null;

        // Метод загрузки картинки с диска
        private void LoadImage()
        {
            try
            {
                // Если путь пустой или файл не существует - сбрасываем картинку
                if (string.IsNullOrEmpty(_goal.ImagePath) || !File.Exists(_goal.ImagePath))
                {
                    GoalImage = null;
                    return;
                }

                // Загружаем картинку
                // Используем FileStream, чтобы не блокировать файл, если вдруг захотим его удалить
                using (var stream = File.OpenRead(_goal.ImagePath))
                {
                    GoalImage = new Bitmap(stream);
                }
            }
            catch (Exception)
            {
                // Если файл битый или ошибка доступа - просто не показываем картинку
                GoalImage = null;
            }
        }

        public DateTime CreatedAt => _goal.CreatedAt;

        // --- Бизнес-методы ---
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

        public void WithdrawDeposit(decimal amount)
        {
            if (amount <= 0 || amount > CurrentAmount) return;
            CurrentAmount -= amount;
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
            CurrentAmount = TargetAmount;
        }

        public void MarkAsIncomplete()
        {
            IsCompleted = false;
        }

        public (bool isValid, string errorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(Title)) return (false, "Название цели не может быть пустым");
            if (TargetAmount <= 0) return (false, "Целевая сумма должна быть больше 0");
            if (Priority < 1 || Priority > 3) return (false, "Приоритет должен быть в диапазоне от 1 до 3");
            return (true, string.Empty);
        }

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