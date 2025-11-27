using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace FinanceFlow.Windows
{
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow()
        {
            InitializeComponent();
        }

        public MessageBoxWindow(string title, string message)
        {
            InitializeComponent();

            this.Title = title; // Заголовок в "шапке" окна

            var messageBlock = this.FindControl<TextBlock>("MessageBlock");
            if (messageBlock != null) messageBlock.Text = message;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}