namespace SparkTrack.Unpacker;

using System;
using Avalonia;
using Avalonia.ReactiveUI;
using CommandLine;
using Fanatiki.MVVM.Extensions;
using NLog;

internal sealed class Program
{
    #region Properties

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static UnpackOptions Options { get; private set; } = null!;

    #endregion

    #region Methods

    [STAThread]
    public static void Main(string[] args)
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        try
        {
            Options = Parser.Default.ParseArguments<UnpackOptions>(args).Value;

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            logger.Fatal(e);
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .UseReactiveUI()
        .UseBootstrapper<UnpackerBootstrapper>();

    #endregion
}