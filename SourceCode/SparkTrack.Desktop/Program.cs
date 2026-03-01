using SparkTrack.AvaloniaImpl.Extensions;

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
    
    #if DEBUG
    private const           string  MutexName = "SparkTrackDebugMutex";
    #else
    private const           string  MutexName = "SparkTrackMutex";
    #endif
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        SetupLogger();
        s_logger.Info("Logger configured");
        
        var deepLink = string.Empty;

        if (args.Length > 0)
        {
            deepLink = args[0];
            s_logger.Info("App started by deeplink: {deeplink}", deepLink);
        }
        
        bool createdNew;

        try
        {
            s_mutex = new Mutex(true, MutexName, out createdNew);
        }
        catch
        {
            createdNew = false;
        }
        
        if (!createdNew)
        {
            // Сообщаем первому инстансу, что нужно показать окно
            s_logger.Info("Found existing app instance, redirecting to one...");
            SingleInstanceIpc.SignalFirstInstance(deepLink);
            return;
        }
        
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        
        s_logger.Info("Starting app v{version}...", typeof(Program).Assembly.GetName().Version);

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
        .LogToNLog()
        .UseReactiveUI()
        .With(() => new Win32PlatformOptions { OverlayPopups = true })
        .With(() => new SkiaOptions { UseOpacitySaveLayer = true, MaxGpuResourceSizeBytes = 512 * 1024 * 1024})
        .UseBootstrapper<SparkTrackBootstrapper>([typeof(App).Assembly]);
    
    private static void SetupLogger()
    {
        NLogConfigManager.EnsureNLogConfig(typeof(App).Assembly, "SparkTrack.AvaloniaImpl.NLog.config");

        LogManager.Setup(cfg => cfg.LoadConfigurationFromFile(NLogConfigManager.NLogConfigPath));
        LogManager.ReconfigExistingLoggers();
    }
}