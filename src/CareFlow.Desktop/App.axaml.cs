using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CareFlow.Desktop.ViewModels;
using CareFlow.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using CareFlow.Infrastructure;

namespace CareFlow.Desktop;

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
            var services = new ServiceCollection();
            services.AddInfrastructure("Data Source=careflow.db");
            _serviceProvider = services.BuildServiceProvider();

            desktop.MainWindow = new MainWindow();
            
            desktop.Exit += (_,_) => _serviceProvider?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private ServiceProvider? _serviceProvider;
}