namespace SparkTrack.Desktop;

using Avalonia;
using Avalonia.ReactiveUI;
using AvaloniaImpl;
using Fanatiki.MVVM.Extensions;
using NLog;
using ReactiveUI;

sealed class Program
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    private static          Mutex?  s_mutex;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        bool createdNew;

        try
        {
            s_mutex = new Mutex(true, "SparkTrackMutex", out createdNew);
        }
        catch
        {
            createdNew = false;
        }
        
        if (!createdNew)
        {
            // Сообщаем первому инстансу, что нужно показать окно
            SingleInstanceIpc.SignalFirstInstance();
            return;
        }
        
        SetupLogger();
        s_logger.Info("Logger configured");
        
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        
        s_logger.Info("Starting app...");

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            s_logger.Info("App shutted down");
        }
        catch (Exception e)
        {
            s_logger.Fatal(e);
            throw;
        }
        finally
        {
            SingleInstanceIpc.Stop();
            s_mutex?.Dispose();
            s_mutex = null;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .UseReactiveUI()
        .With(() => new SkiaOptions { UseOpacitySaveLayer = true, MaxGpuResourceSizeBytes = 512 * 1024 * 1024})
        .UseBootstrapper<SparkTrackBootstrapper>([typeof(App).Assembly]);
    
    private static void SetupLogger()
    {
        NLogConfigManager.EnsureNLogConfig(typeof(App).Assembly, "SparkTrack.AvaloniaImpl.NLog.config");

        LogManager.Setup(cfg => cfg.LoadConfigurationFromFile(NLogConfigManager.NLogConfigPath));
        LogManager.ReconfigExistingLoggers();
    }
}