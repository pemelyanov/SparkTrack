using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace SparkTrack.AvaloniaImpl;

using System.Globalization;
using Windows.Main;
using Avalonia.Controls;
using Avalonia.Threading;
using Core.Shared.Eventing;
using Events;
using Splat;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
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
            
            SingleInstanceIpc.StartListening(() =>
            {
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