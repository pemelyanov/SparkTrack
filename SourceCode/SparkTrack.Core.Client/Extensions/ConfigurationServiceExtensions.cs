namespace SparkTrack.Core.Client.Extensions;

using Services.Configuration;

public static class ConfigurationServiceExtensions
{
    public static void Update<TConfig>(this IConfigurationService<TConfig> service, Func<TConfig, TConfig> update) =>
        service.UpdateConfig(update(service.Config));
}