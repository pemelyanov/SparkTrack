using System.Collections.Concurrent;
using Avalonia.Logging;
using NLog;
using NLog.Layouts;
using ILogger = NLog.ILogger;
using LogLevel = NLog.LogLevel;

namespace SparkTrack.AvaloniaImpl.Logging;

using NLog.Config;

public class NLogSink : ILogSink
{
    #region Fields

    private LogEventLevel                             m_logEventLevel;
    private readonly        HashSet<string>?                          m_areas;
    private static readonly ConcurrentDictionary<string, NLog.Logger> s_loggerCache = new();

    #endregion

    #region LifeCycle

    public NLogSink(
        IList<string>? areas = null)
    {
        LogManager.ConfigurationChanged += LogManager_OnConfigurationChanged;
        
        InitializeMinLogLevel();
        
        m_areas = areas?.Count > 0 ? new HashSet<string>(areas) : null;
    }

    #endregion

    #region Methods

    public bool IsEnabled(LogEventLevel level, string area)
    {
        return level >= m_logEventLevel && (m_areas?.Contains(area) ?? true);
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        if (!IsEnabled(level, area)) return;

        var logger = Resolve(source?.GetType(), area);
        logger.Log(GetLogLevel(level), messageTemplate);
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate,
        params object?[] propertyValues)
    {
        if (!IsEnabled(level, area)) return;

        var logger = Resolve(source?.GetType(), area);
        logger.Log(GetLogLevel(level), messageTemplate, propertyValues);
    }

    public static ILogger Resolve(Type? source, string? area)
    {
        var loggerName = source?.ToString() ?? area;
        if (string.IsNullOrEmpty(loggerName))
            loggerName = typeof(NLogSink).ToString();

        if (!s_loggerCache.TryGetValue(loggerName, out var logger))
        {
            logger = LogManager.GetLogger(loggerName);
            s_loggerCache.TryAdd(loggerName, logger);
        }

        return logger;
    }

    private static LogLevel GetLogLevel(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => LogLevel.Trace,
        LogEventLevel.Debug => LogLevel.Debug,
        LogEventLevel.Information => LogLevel.Info,
        LogEventLevel.Warning => LogLevel.Warn,
        LogEventLevel.Error => LogLevel.Error,
        LogEventLevel.Fatal => LogLevel.Fatal,
        _ => LogLevel.Trace
    };
    
    private static LogEventLevel GetLogLevel(string level) => level switch
    {
        "Trace" => LogEventLevel.Verbose,
        "Debug" => LogEventLevel.Debug,
        "Info" => LogEventLevel.Information,
        "Warn" => LogEventLevel.Warning,
        "Error" => LogEventLevel.Error,
        "Fatal" => LogEventLevel.Fatal,
        _ => LogEventLevel.Verbose 
    };
    
    private void InitializeMinLogLevel()
    {
        if(LogManager.Configuration is null) return;
        
        if(LogManager.Configuration.Variables.TryGetValue("avaloniaMinLogLevel", out var minLogLevel))
            m_logEventLevel = GetLogLevel(minLogLevel.ToString()!);
        else
            m_logEventLevel = LogEventLevel.Warning;
    }

    private void LogManager_OnConfigurationChanged(object? sender, LoggingConfigurationChangedEventArgs e)
    {
        InitializeMinLogLevel();
    }

    #endregion
}