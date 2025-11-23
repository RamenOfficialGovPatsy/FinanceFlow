using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Avalonia.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;

namespace FinanceFlow.ViewModels
{
    public class AddEditGoalViewModel : ViewModelBase
    {
        private readonly IGoalService _goalService;
        private readonly bool _isEditMode;
        private readonly int _editingGoalId;

        public event Action? RequestClose;

        public string WindowTitle => _isEditMode ? "Редактирование цели" : "Новая цель";

        private string _title = string.Empty;
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        private GoalCategory? _selectedCategory;
        public GoalCategory? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }

        private PriorityItem? _selectedPriority;
        public PriorityItem? SelectedPriority { get => _selectedPriority; set => SetProperty(ref _selectedPriority, value); }

        private decimal _targetAmount;
        public decimal TargetAmount { get => _targetAmount; set => SetProperty(ref _targetAmount, value); }

        private decimal _currentAmount;
        public decimal CurrentAmount { get => _currentAmount; set => SetProperty(ref _currentAmount, value); }

        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime _endDate = DateTime.Today.AddMonths(3);
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        private string _description = string.Empty;
        public string Description { get => _description; set => SetProperty(ref _description, value); }

        private string? _imagePath;
        public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }

        private Bitmap? _goalImage;
        public Bitmap? GoalImage
        {
            get => _goalImage;
            set { if (SetProperty(ref _goalImage, value)) OnPropertyChanged(nameof(HasImage)); }
        }
        public bool HasImage => GoalImage != null;

        public ObservableCollection<GoalCategory> Categories { get; } = new();
        public ObservableCollection<PriorityItem> Priorities { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteImageCommand { get; }

        public AddEditGoalViewModel()
        {
            _goalService = null!;
            InitializePriorities();
            SaveCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            CancelCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            DeleteImageCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        }

        public AddEditGoalViewModel(IGoalService goalService, Goal? goalToEdit = null)
        {
            _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));
            InitializePriorities();
            _ = LoadCategoriesAsync(goalToEdit);

            if (goalToEdit != null)
            {
                _isEditMode = true;
                _editingGoalId = goalToEdit.GoalId;
            }

            SaveCommand = new AsyncRelayCommand(SaveGoalAsync);
            CancelCommand = new AsyncRelayCommand(() => { RequestClose?.Invoke(); return Task.CompletedTask; });

            DeleteImageCommand = new AsyncRelayCommand(() =>
            {
                GoalImage = null;
                ImagePath = null;
                OnPropertyChanged(nameof(HasImage)); // Принудительно обновляем UI
                return Task.CompletedTask;
            });
        }

        private void InitializePriorities()
        {
            Priorities.Add(new PriorityItem { Value = 1, Name = "Высокий", Color = "#EF4444" });
            Priorities.Add(new PriorityItem { Value = 2, Name = "Средний", Color = "#F59E0B" });
            Priorities.Add(new PriorityItem { Value = 3, Name = "Низкий", Color = "#10B981" });
            SelectedPriority = Priorities.FirstOrDefault(p => p.Value == 2);
        }

        private async Task LoadCategoriesAsync(Goal? goalToEdit)
        {
            if (_goalService == null) return;
            var categoriesFromDb = await _goalService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var cat in categoriesFromDb) Categories.Add(cat);

            if (goalToEdit != null) FillFormData(goalToEdit);
            else SelectedCategory = Categories.FirstOrDefault();
        }

        private void FillFormData(Goal goal)
        {
            Console.WriteLine($"FillFormData: Title={goal.Title}, EndDate={goal.EndDate}, Kind={goal.EndDate.Kind}");

            Title = goal.Title;
            TargetAmount = goal.TargetAmount;
            CurrentAmount = goal.CurrentAmount;

            // --- FIX: Защита от пустых дат (0001-01-01) ---
            // Если дата сломана, ставим дефолтную, чтобы форма не глючила
            StartDate = goal.StartDate == DateTime.MinValue ? DateTime.Today : goal.StartDate;
            EndDate = goal.EndDate == DateTime.MinValue ? DateTime.Today.AddMonths(3) : goal.EndDate;
            // ----------------------------------------------

            Description = goal.Description ?? string.Empty;
            SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == goal.CategoryId);
            SelectedPriority = Priorities.FirstOrDefault(p => p.Value == goal.Priority);

            if (!string.IsNullOrEmpty(goal.ImagePath)) SetImage(goal.ImagePath);
        }

        public void SetImage(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    GoalImage = new Bitmap(path);
                    ImagePath = path;
                    OnPropertyChanged(nameof(HasImage));
                }
            }
            catch (Exception ex) { Console.WriteLine($"Ошибка картинки: {ex.Message}"); }
        }

        private async Task SaveGoalAsync()
        {
            if (!Validate()) return;
            try
            {
                var goal = new Goal
                {
                    GoalId = _isEditMode ? _editingGoalId : 0,
                    Title = Title,
                    CategoryId = SelectedCategory!.CategoryId,
                    Priority = SelectedPriority!.Value,
                    TargetAmount = TargetAmount,
                    CurrentAmount = CurrentAmount,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Description = Description,
                    ImagePath = ImagePath
                };

                (bool success, string message) result;
                if (_isEditMode) result = await _goalService.UpdateGoalAsync(goal);
                else result = await _goalService.AddGoalAsync(goal);

                if (result.success) RequestClose?.Invoke();
                else Console.WriteLine($"Ошибка: {result.message}");
            }
            catch (Exception ex) { Console.WriteLine($"Критическая ошибка: {ex.Message}"); }
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Title)) return false;
            if (TargetAmount <= 0) return false;
            if (SelectedCategory == null) return false;
            if (SelectedPriority == null) return false;
            if (EndDate <= StartDate) return false;
            return true;
        }
    }

    public class PriorityItem
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}