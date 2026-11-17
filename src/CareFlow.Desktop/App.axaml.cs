using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CareFlow.Application.Interfaces;
using CareFlow.Desktop.ViewModels;
using CareFlow.Desktop.Views;
using CareFlow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;

namespace CareFlow.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            services.AddInfrastructure("Data Source=careflow.db");
            services.AddScoped<IBookingService, BookingService>();
            _serviceProvider = services.BuildServiceProvider();

            desktop.MainWindow = new MainWindow();
            
            desktop.Exit += (_,_) => _serviceProvider?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private ServiceProvider? _serviceProvider;
}