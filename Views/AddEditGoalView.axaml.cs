using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage; // Для работы с файлами
using FinanceFlow.ViewModels;
using System.Collections.Generic;

namespace FinanceFlow.Views
{
    public partial class AddEditGoalView : UserControl
    {
        public AddEditGoalView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Обработчик загрузки изображения
        private async void LoadImageButton_Click(object? sender, RoutedEventArgs e)
        {
            // Получаем доступ к окну
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Открываем диалог выбора файла
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите изображение цели",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    FilePickerFileTypes.ImageAll // Разрешаем все форматы изображений
                }
            });

            if (files.Count >= 1)
            {
                // Получаем путь к файлу
                // В Avalonia 11 путь может быть URI, поэтому приводим к LocalPath
                var filePath = files[0].Path.LocalPath;

                // Передаем путь во ViewModel
                if (DataContext is AddEditGoalViewModel vm)
                {
                    vm.SetImage(filePath);
                }
            }
        }

        // Обработчик кнопки "Отмена"
        /*
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            if (VisualRoot is Window parentWindow)
            {
                parentWindow.Close();
            }
        }
        */

        // Обработчик кнопки "Сохранить"
        /*
        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Кнопка 'Сохранить' нажата");
        }
        */
    }
}