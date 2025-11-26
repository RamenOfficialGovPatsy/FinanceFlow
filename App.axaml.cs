using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FinanceFlow.Data;
using FinanceFlow.Services.Implementations;
using FinanceFlow.Services.Interfaces;
using FinanceFlow.ViewModels;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure; // Важно для лицензии

namespace FinanceFlow;

public partial class App : Application
{
    // Контейнер для сервисов
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        this.EnableHotReload(); // Горячая перезагрузка

        // Установка бесплатной лицензии для QuestPDF
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. Настраиваем DI (Dependency Injection)
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 2. Получаем ViewModel из контейнера (он сам подставит нужные сервисы)
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // База данных (Контекст)
        services.AddDbContext<AppDbContext>();

        // Логирование
        services.AddLogging(builder => builder.AddConsole());

        // --- СЕРВИСЫ (Бизнес-логика) ---
        services.AddTransient<IGoalService, GoalService>();
        services.AddTransient<IDepositService, DepositService>();
        services.AddTransient<IAnalyticsService, AnalyticsService>(); // Раскомментировано

        // --- VIEWMODELS ---
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<AddEditGoalViewModel>(); // Раскомментировано
        services.AddTransient<AnalyticsViewModel>();   // Добавлено для аналитики
        services.AddTransient<DepositViewModel>();     // Добавлено для пополнений
    }
}