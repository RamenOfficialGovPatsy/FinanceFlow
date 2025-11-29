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
using QuestPDF.Infrastructure;

namespace FinanceFlow;

public partial class App : Application
{
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
        //  Настройка DI (Dependency Injection)
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            //  Получаю ViewModel из контейнера
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
        services.AddDbContext<AppDbContext>();

        // Логирование
        services.AddLogging(builder => builder.ClearProviders());

        services.AddTransient<IGoalService, GoalService>();
        services.AddTransient<IDepositService, DepositService>();
        services.AddTransient<IAnalyticsService, AnalyticsService>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<AddEditGoalViewModel>();
        services.AddTransient<AnalyticsViewModel>();
        services.AddTransient<DepositViewModel>();
    }
}