using FinanceFlow.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class AddEditGoalViewModel : ViewModelBase
    {
        private bool _isEditMode;
        private Goal? _editingGoal;

        public string WindowTitle => _isEditMode ? "Редактирование цели" : "Новая цель";

        // Свойства формы
        public string Title { get; set; } = string.Empty;
        public int SelectedCategoryId { get; set; } = 1;
        public int SelectedPriority { get; set; } = 2; // Средний по умолчанию
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);
        public string Description { get; set; } = string.Empty;

        // Списки для ComboBox
        public ObservableCollection<GoalCategory> Categories { get; } = new();
        public ObservableCollection<PriorityItem> Priorities { get; } = new();

        // Команды
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEditGoalViewModel(bool isEditMode = false, Goal? goalToEdit = null)
        {
            _isEditMode = isEditMode;
            _editingGoal = goalToEdit;

            // Заполняем списки
            InitializeCategories();
            InitializePriorities();

            // Если режим редактирования - заполняем данные
            if (_isEditMode && _editingGoal != null)
            {
                LoadGoalData();
            }

            SaveCommand = new AsyncRelayCommand(SaveGoalAsync);
            CancelCommand = new AsyncRelayCommand(() => Task.CompletedTask); // Временная заглушка
        }

        private void InitializeCategories()
        {
            // Временно хардкод, позже загрузим из БД
            Categories.Add(new GoalCategory { CategoryId = 1, Name = "Техника", Icon = "📱" });
            Categories.Add(new GoalCategory { CategoryId = 2, Name = "Авто", Icon = "🚗" });
            Categories.Add(new GoalCategory { CategoryId = 3, Name = "Путешествия", Icon = "✈️" });
            Categories.Add(new GoalCategory { CategoryId = 4, Name = "Образование", Icon = "🎓" });
        }

        private void InitializePriorities()
        {
            Priorities.Add(new PriorityItem { Value = 1, Name = "Высокий", Color = "#EF4444" });
            Priorities.Add(new PriorityItem { Value = 2, Name = "Средний", Color = "#F59E0B" });
            Priorities.Add(new PriorityItem { Value = 3, Name = "Низкий", Color = "#10B981" });
        }



        private void LoadGoalData()
        {
            if (_editingGoal == null) return;

            Title = _editingGoal.Title;
            SelectedCategoryId = _editingGoal.CategoryId;
            SelectedPriority = _editingGoal.Priority;
            TargetAmount = _editingGoal.TargetAmount;
            CurrentAmount = _editingGoal.CurrentAmount;
            StartDate = _editingGoal.StartDate;
            EndDate = _editingGoal.EndDate;
            Description = _editingGoal.Description ?? string.Empty;
        }

        private async Task SaveGoalAsync()
        {
            // Временно заглушка - позже подключим сервисы
            await Task.Delay(100);
            Console.WriteLine($"Сохранение цели: {Title}");
        }
    }



    public class PriorityItem
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}