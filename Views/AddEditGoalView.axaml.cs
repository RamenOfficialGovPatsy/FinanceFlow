using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using FinanceFlow.ViewModels;

namespace FinanceFlow.Views
{
    public partial class AddEditGoalView : UserControl
    {
        // Ограничения для загружаемых изображений
        // Максимальный размер файла - 15 МБ
        private const long MaxFileSize = 15 * 1024 * 1024;

        // Разрешенные форматы
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".webp" };
        public AddEditGoalView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Обработчик клика по пустому месту
        // Сбрасывает фокус с активного элемента при клике на фон
        private void Root_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            // Получаем доступ к верхнему уровню окна
            var topLevel = TopLevel.GetTopLevel(this);

            // Сбрасываем фокус с любого активного элемента ввода
            topLevel?.FocusManager?.ClearFocus();
        }

        // Обработчик кнопки загрузки изображения для цели
        private async void LoadImageButton_Click(object? sender, RoutedEventArgs e)
        {
            // Получаем ссылку на верхний уровень для доступа к диалоговым окнам
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Открываем диалог выбора файла с настройками
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите изображение цели",
                AllowMultiple = false, // Запрещаем множественный выбор
                FileTypeFilter = new List<FilePickerFileType>
                {
                    // Показываем только изображения
                    FilePickerFileTypes.ImageAll
                }
            });

            // Проверяем что пользователь выбрал файл
            if (files.Count >= 1)
            {
                // Получаем локальный путь к выбранному файлу
                var filePath = files[0].Path.LocalPath;

                if (DataContext is AddEditGoalViewModel vm)
                {
                    try
                    {
                        // Создаем объект FileInfo для проверки свойств файла
                        var fileInfo = new FileInfo(filePath);

                        // Получаем доступ к ViewModel для передачи данных

                        // Получаем расширение файла в нижнем регистре
                        var extension = fileInfo.Extension.ToLowerInvariant();

                        // Проверяем что расширение в списке разрешенных
                        if (!_allowedExtensions.Contains(extension))
                        {
                            // Показываем ошибку если формат не поддерживается
                            vm.TriggerError("Неподдерживаемый формат файла.\nРазрешены: JPG, PNG, BMP, WEBP.", "Ошибка файла");
                            return;
                        }

                        // Сравниваем размер файла с максимально допустимым
                        if (fileInfo.Length > MaxFileSize)
                        {
                            // Показываем ошибку с информацией о размерах
                            vm.TriggerError($"Файл слишком большой ({fileInfo.Length / 1024 / 1024} МБ).\nМаксимальный размер: 15 МБ.", "Ошибка размера");
                            return;
                        }

                        // Если все ок - устанавливаем картинку
                        vm.SetImage(filePath);
                        //  Console.WriteLine($"[Успех] Изображение загружено: {fileInfo.Name}");

                    }
                    catch (Exception ex)
                    {
                        vm.TriggerError($"Не удалось открыть файл: {ex.Message}", "Ошибка чтения");
                    }
                }
            }
        }
    }
}