namespace SparkTrack.Core.Client.Services.Configuration;

public interface IConfigurationService<TConfig>
{
    event Action<TConfig> ConfigChanged;
    
    TConfig Config { get; }

    void UpdateConfig(TConfig config);
}