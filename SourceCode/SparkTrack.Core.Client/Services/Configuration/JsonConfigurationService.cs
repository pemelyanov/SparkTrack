namespace SparkTrack.Core.Client.Services.Configuration;

using System.Text.Json;
using NLog;

public class JsonConfigurationService<TData> : IConfigurationService<TData> where TData : new()
{
    private readonly ILogger m_logger = LogManager.GetCurrentClassLogger();

    private readonly string                m_configFilePath;
    private readonly JsonSerializerOptions m_jsonOptions;
    private          TData                 m_config;
    
    public event Action<TData>? ConfigChanged;

    public JsonConfigurationService(string configPath)
    {
        m_configFilePath = configPath;
        m_jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        m_config = LoadConfig();
    }

    public TData Config => m_config;

    public void UpdateConfig(TData config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        try
        {
            SaveConfig(config);
            m_config = config;
            ConfigChanged?.Invoke(config);
        }
        catch (Exception ex)
        {
            m_logger.Error(ex, "Failed to update configuration: {ConfigPath}", m_configFilePath);
            throw;
        }
    }

    private TData LoadConfig()
    {
        try
        {
            if (!File.Exists(m_configFilePath))
            {
                m_logger.Warn("Config file not found, creating default configuration: {ConfigPath}", m_configFilePath);
                var defaultConfig = new TData();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            var json = File.ReadAllText(m_configFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                m_logger.Warn("Config file is empty, using default configuration: {ConfigPath}", m_configFilePath);
                return new TData();
            }

            var config = JsonSerializer.Deserialize<TData>(json, m_jsonOptions);
            if (config == null)
            {
                m_logger.Warn(
                    "Failed to deserialize config, using default configuration: {ConfigPath}",
                    m_configFilePath
                );
                return new TData();
            }

            m_logger.Info("Configuration loaded successfully: {ConfigPath}", m_configFilePath);
            return config;
        }
        catch (JsonException ex)
        {
            m_logger.Error(
                ex,
                "JSON deserialization error, using default configuration: {ConfigPath}",
                m_configFilePath
            );
            return new TData();
        }
        catch (Exception ex)
        {
            m_logger.Error(ex, "Failed to load configuration: {ConfigPath}", m_configFilePath);
            return new TData();
        }
    }

    private void SaveConfig(TData config)
    {
        try
        {
            var directory = Path.GetDirectoryName(m_configFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                m_logger.Debug("Created config directory: {Directory}", directory);
            }

            var json = JsonSerializer.Serialize(config, m_jsonOptions);
            File.WriteAllText(m_configFilePath, json);
            m_logger.Debug("Configuration saved successfully: {ConfigPath}", m_configFilePath);
        }
        catch (Exception ex)
        {
            m_logger.Error(ex, "Failed to save configuration: {ConfigPath}", m_configFilePath);
            throw;
        }
    }
}