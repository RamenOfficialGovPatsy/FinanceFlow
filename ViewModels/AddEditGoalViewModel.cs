using FinanceFlow.Models;
using FinanceFlow.Services.Interfaces;
using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class AddEditGoalViewModel : ViewModelBase
    {
        private readonly IGoalService _goalService;
        private readonly bool _isEditMode;
        private readonly int _editingGoalId;

        // Публичный метод для вызова ошибки из View
        public void TriggerError(string message, string title) => ShowError(message, title);

        public event Action? RequestClose;

        // Динамический заголовок окна в зависимости от режима
        public string WindowTitle => _isEditMode ? "Редактирование цели" : "Новая цель";

        // Основные свойства цели с уведомлениями об изменении
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private GoalCategory? _selectedCategory;
        public GoalCategory? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }

        private PriorityItem? _selectedPriority;
        public PriorityItem? SelectedPriority { get => _selectedPriority; set => SetProperty(ref _selectedPriority, value); }

        // Суммы с nullable для удобства ввода
        private decimal? _targetAmount = 1;
        public decimal? TargetAmount
        {
            get => _targetAmount;
            set => SetProperty(ref _targetAmount, value);
        }

        private decimal? _currentAmount;
        public decimal? CurrentAmount
        {
            get => _currentAmount;
            set => SetProperty(ref _currentAmount, value);
        }

        // Даты с разумными значениями по умолчанию
        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime _endDate = DateTime.Today.AddMonths(3);
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        private string _description = string.Empty;
        public string Description { get => _description; set => SetProperty(ref _description, value); }

        // Путь к изображению и само изображение
        private string? _imagePath;
        public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }

        private Bitmap? _goalImage;
        public Bitmap? GoalImage
        {
            get => _goalImage;
            set { if (SetProperty(ref _goalImage, value)) OnPropertyChanged(nameof(HasImage)); }
        }
        public bool HasImage => GoalImage != null;

        // Коллекции для выпадающих списков
        public ObservableCollection<GoalCategory> Categories { get; } = new();
        public ObservableCollection<PriorityItem> Priorities { get; } = new();

        // Команды для кнопок интерфейса
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteImageCommand { get; }

        // Конструктор для дизайнера 
        public AddEditGoalViewModel()
        {
            _goalService = null!;
            InitializePriorities();
            SaveCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            CancelCommand = new AsyncRelayCommand(() => Task.CompletedTask);
            DeleteImageCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        }

        // Основной конструктор - инициализирует все зависимости
        public AddEditGoalViewModel(IGoalService goalService, Goal? goalToEdit = null)
        {
            _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));
            InitializePriorities();
            _ = LoadCategoriesAsync(goalToEdit); // Загружаю категории асинхронно

            // Определяю режим работы: редактирование или создание
            if (goalToEdit != null)
            {
                _isEditMode = true;
                _editingGoalId = goalToEdit.GoalId;
            }

            // Настраиваю команды с реальной логикой
            SaveCommand = new AsyncRelayCommand(SaveGoalAsync);
            CancelCommand = new AsyncRelayCommand(() => { RequestClose?.Invoke(); return Task.CompletedTask; });

            DeleteImageCommand = new AsyncRelayCommand(() =>
            {
                GoalImage = null;
                ImagePath = null;
                OnPropertyChanged(nameof(HasImage));
                return Task.CompletedTask;
            });
        }

        // Заполняю список приоритетов с цветами для красивого отображения
        private void InitializePriorities()
        {
            Priorities.Add(new PriorityItem { Value = 1, Name = "Высокий", Color = "#EF4444" });
            Priorities.Add(new PriorityItem { Value = 2, Name = "Средний", Color = "#F59E0B" });
            Priorities.Add(new PriorityItem { Value = 3, Name = "Низкий", Color = "#10B981" });

            // Средний по умолчанию
            SelectedPriority = Priorities.FirstOrDefault(p => p.Value == 2);
        }

        // Загружаю категории из базы и заполняю форму если редактирую
        private async Task LoadCategoriesAsync(Goal? goalToEdit)
        {
            if (_goalService == null) return;
            var categoriesFromDb = await _goalService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var cat in categoriesFromDb) Categories.Add(cat);

            // Если редактирую существующую цель - заполняю форму её данными
            if (goalToEdit != null) FillFormData(goalToEdit);
            else SelectedCategory = Categories.FirstOrDefault(); // Иначе выбираю 
            // первую категорию
        }

        // Заполняю форму данными из существующей цели
        private void FillFormData(Goal goal)
        {
            Title = goal.Title;
            TargetAmount = goal.TargetAmount;
            CurrentAmount = goal.CurrentAmount;
            StartDate = goal.StartDate == DateTime.MinValue ? DateTime.Today : goal.StartDate;
            EndDate = goal.EndDate == DateTime.MinValue ? DateTime.Today.AddMonths(3) : goal.EndDate;
            Description = goal.Description ?? string.Empty;
            SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == goal.CategoryId);
            SelectedPriority = Priorities.FirstOrDefault(p => p.Value == goal.Priority);

            // Загружаю изображение если оно есть
            if (!string.IsNullOrEmpty(goal.ImagePath)) SetImage(goal.ImagePath);
        }

        // Устанавливаю изображение цели с обработкой ошибок
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
            catch (Exception ex)
            {
                ShowError($"Не удалось загрузить изображение: {ex.Message}", "Ошибка изображения");
            }
        }

        // Основной метод сохранения цели
        private async Task SaveGoalAsync()
        {
            // Сначала проверяю валидность введенных данных
            var (isValid, error, title) = Validate();

            if (!isValid)
            {
                ShowError(error, title);
                return;
            }

            try
            {
                decimal current = CurrentAmount ?? 0;
                decimal target = TargetAmount ?? 0;

                // Проверка бизнес-правила (Суммы) перед созданием объекта
                if (current > target)
                {
                    ShowError($"Текущая сумма ({current:N0}) не может быть больше целевой ({target:N0}).", "Ошибка суммы");
                    return;
                }

                // Создаю объект цели с данными из формы
                var goal = new Goal
                {
                    GoalId = _isEditMode ? _editingGoalId : 0,
                    Title = Title,
                    CategoryId = SelectedCategory!.CategoryId,
                    Priority = SelectedPriority!.Value,
                    TargetAmount = target,
                    CurrentAmount = current,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Description = Description,
                    ImagePath = ImagePath
                };

                (bool success, string message) result;

                // Вызываю соответствующий метод сервиса в зависимости от режима
                if (_isEditMode)
                    result = await _goalService.UpdateGoalAsync(goal);
                else
                    result = await _goalService.AddGoalAsync(goal);

                if (result.success)
                {
                    RequestClose?.Invoke(); // Успешно - закрываю окно
                }
                else
                {
                    // Обрабатываю разные типы ошибок от сервиса
                    if (result.message.Contains("CK_Goals_Amounts"))
                        ShowError("Текущая сумма превышает целевую.", "Ошибка данных");
                    else
                        ShowError(result.message, "Ошибка сохранения");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Критическая ошибка: {ex.Message}", "Сбой программы");
            }
        }

        // Валидация данных формы перед сохранением
        private (bool isValid, string error, string title) Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return (false, "Введите название цели.", "Название не указано");

            // Проверяю что целевая сумма указана и положительная
            if ((TargetAmount ?? 0) <= 0)
                return (false, "Целевая сумма должна быть больше 0.", "Некорректная сумма");

            // Проверяю логику дат
            if (EndDate.Date < StartDate.Date)
                return (false, "Дата окончания не может быть раньше даты начала.", "Ошибка в датах");

            return (true, string.Empty, string.Empty);
        }
    }

    // Вспомогательный класс для отображения приоритетов в выпадающем списке
    public class PriorityItem
    {
        public int Value { get; set; } // Числовое значение для базы
        public string Name { get; set; } = string.Empty; // Отображаемое имя
        public string Color { get; set; } = string.Empty; // Цвет для визуального отличия
    }
}