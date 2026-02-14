namespace SparkTrack.Unpacker;

using Windows.Main;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NLog;
using Splat;
using MainWindow = Windows.Main.MainWindow;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        var config = new NLog.Config.XmlLoggingConfiguration("NLog.Unpacker.config");
        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Locator.Current.GetService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}