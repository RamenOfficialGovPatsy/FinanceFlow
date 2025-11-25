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

                    // 1. ПРОВЕРКА ФОРМАТА (РАСШИРЕНИЯ)
                    var extension = fileInfo.Extension.ToLowerInvariant(); // .jpg
                    if (!_allowedExtensions.Contains(extension))
                    {
                        Console.WriteLine($"[Ошибка] Неподдерживаемый формат: {extension}. Разрешены: JPG, PNG, BMP, WEBP.");
                        // Здесь потом будет MessageBox.Show("Неверный формат...");
                        return;
                    }

                    // 2. ПРОВЕРКА РАЗМЕРА
                    if (fileInfo.Length > MaxFileSize)
                    {
                        Console.WriteLine($"[Ошибка] Файл слишком большой: {fileInfo.Length / 1024 / 1024} МБ. Лимит: 15 МБ.");
                        // Здесь потом будет MessageBox.Show("Файл слишком большой...");
                        return;
                    }

                    // Если все проверки пройдены - передаем во ViewModel
                    if (DataContext is AddEditGoalViewModel vm)
                    {
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