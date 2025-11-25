using Avalonia.Controls;
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

        private void Root_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            topLevel?.FocusManager?.ClearFocus();
        }
    }
}