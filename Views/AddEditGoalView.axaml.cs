using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using FinanceFlow.ViewModels;

namespace FinanceFlow.Views
{
    public partial class AddEditGoalView : UserControl
    {
        private const long MaxFileSize = 15 * 1024 * 1024;
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
        private void Root_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            // Получаем доступ к окну и сбрасываем фокус
            var topLevel = TopLevel.GetTopLevel(this);
            topLevel?.FocusManager?.ClearFocus();
        }

        // Обработчик загрузки изображения
        private async void LoadImageButton_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите изображение цели",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    FilePickerFileTypes.ImageAll
                }
            });

            if (files.Count >= 1)
            {
                var filePath = files[0].Path.LocalPath;

                try
                {
                    var fileInfo = new FileInfo(filePath);

                    // Получаем доступ к ViewModel
                    if (DataContext is AddEditGoalViewModel vm)
                    {
                        // 1. ПРОВЕРКА ФОРМАТА
                        var extension = fileInfo.Extension.ToLowerInvariant();
                        if (!_allowedExtensions.Contains(extension))
                        {
                            vm.TriggerError("Неподдерживаемый формат файла.\nРазрешены: JPG, PNG, BMP, WEBP.", "Ошибка файла");
                            return;
                        }

                        // 2. ПРОВЕРКА РАЗМЕРА
                        if (fileInfo.Length > MaxFileSize)
                        {
                            vm.TriggerError($"Файл слишком большой ({fileInfo.Length / 1024 / 1024} МБ).\nМаксимальный размер: 15 МБ.", "Ошибка размера");
                            return;
                        }

                        // Если все ок - устанавливаем картинку
                        vm.SetImage(filePath);
                        Console.WriteLine($"[Успех] Изображение загружено: {fileInfo.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка проверки файла: {ex.Message}");
                }
            }
        }
    }
}