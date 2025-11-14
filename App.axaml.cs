using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
//using FinanceFlow.ViewModels;
//using FinanceFlow.Windows;

namespace FinanceFlow;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            /*
            // Эту строку мы раскомментируем на следующем шаге,
            // когда создадим MainWindowViewModel.
            
            desktop.MainWindow.DataContext = new MainWindowViewModel();
            */
        }

        base.OnFrameworkInitializationCompleted();
    }
}