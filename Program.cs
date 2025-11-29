using Avalonia;

namespace FinanceFlow;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Конфигурация Avalonia
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}