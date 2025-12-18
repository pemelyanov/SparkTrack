namespace SparkTrack.Core.Client.Services.Configuration;

public interface IConfigurationService<TConfig>
{
    TConfig Config { get; }

    void UpdateConfig(TConfig config);
}