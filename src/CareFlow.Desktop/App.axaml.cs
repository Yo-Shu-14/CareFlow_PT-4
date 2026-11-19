using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CareFlow.Desktop.ViewModels;
using CareFlow.Desktop.Views;
using CareFlow.Infrastructure;
using CareFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareFlow.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;

    public static ServiceProvider? Services { get; private set; }

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
            services.AddTransient<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();
            Services = _serviceProvider;

            // Apply pending migrations on startup to ensure reproducible DB initialization
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                db.Database.Migrate();
            }

            var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = viewModel };

            desktop.Exit += (_, _) =>
            {
                _serviceProvider?.Dispose();
                Services = null;
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
