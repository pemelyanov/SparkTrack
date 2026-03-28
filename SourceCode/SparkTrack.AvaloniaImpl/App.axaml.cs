using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace SparkTrack.AvaloniaImpl;

using System.Globalization;
using Windows.Main;
using Avalonia.Controls;
using Avalonia.Threading;
using Core.Shared.Eventing;
using DeepLink;
using Events;
using NLog;
using Services.DeepLinkNavigation;
using Splat;
using ILogger = NLog.ILogger;

public partial class App : Application
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    public static SparkTrackDeepLink? StartupDeepLink { get; set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var deepLinkNavigationService = Locator.Current.GetService<IDeepLinkNavigationService>();
        
        if(StartupDeepLink is not null)
            deepLinkNavigationService?.Enqueue(StartupDeepLink);
        
        var eventEmitter = Locator.Current.GetService<IEventEmitter>()!;

        await eventEmitter.RaiseAsync(new StartupEvent());
        
        var ruCulture = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.CurrentCulture = ruCulture;
        CultureInfo.CurrentUICulture = ruCulture;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = Locator.Current.GetService<MainWindow>()!;
            mainWindow.DataContext = Locator.Current.GetService<MainWindowViewModel>();
            desktop.MainWindow = mainWindow;
            
            SingleInstanceIpc.StartListening(deeplink =>
            {
                try
                {
                    deepLinkNavigationService?.Enqueue( SparkTrackDeepLink.Parse(deeplink));
                }
                catch (Exception e)
                {
                    s_logger.Warn(e, "Error handling deeplink");
                }
                
                Dispatcher.UIThread.Post(() =>
                {
                    if (mainWindow.WindowState == WindowState.Minimized)
                        mainWindow.WindowState = WindowState.Normal;

                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.Topmost = true;
                    mainWindow.Topmost = false;
                });
            });
        }

        base.OnFrameworkInitializationCompleted();
    }
}