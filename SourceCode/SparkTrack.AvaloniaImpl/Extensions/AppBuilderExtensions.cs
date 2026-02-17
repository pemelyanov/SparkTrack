using Avalonia;
using Avalonia.Logging;
using SparkTrack.AvaloniaImpl.Logging;

namespace SparkTrack.AvaloniaImpl.Extensions;

public static class AppBuilderExtensions
{
    public static AppBuilder LogToNLog(
        this AppBuilder builder,
        params string[] areas)
    {
        Logger.Sink = new NLogSink(areas);

        return builder;
    }
}