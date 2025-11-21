using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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

        // Обработчик кнопки "Отмена"
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            if (VisualRoot is Window parentWindow)
            {
                parentWindow.Close();
            }
        }

        // Обработчик кнопки "Сохранить"
        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Кнопка 'Сохранить' нажата");
        }
    }
}