using Avalonia;
using System;

namespace FinanceFlow;

class Program
{
    // Точка входа в приложение
    [STAThread]
    public static void Main(string[] args)
    {
        // ВАЖНО: Это должно быть самой первой строчкой кода в программе!
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