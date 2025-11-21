using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace FinanceFlow.Views
{
    public partial class DepositView : UserControl
    {
        public DepositView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            if (VisualRoot is Window parentWindow)
            {
                parentWindow.Close();
            }
        }

        private void AddButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Кнопка 'Добавить' нажата");
        }
    }
}