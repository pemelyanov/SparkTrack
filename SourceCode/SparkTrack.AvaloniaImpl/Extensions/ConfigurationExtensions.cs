using Microsoft.Extensions.Configuration;

namespace SparkTrack.AvaloniaImpl.Extensions;

public static class ConfigurationExtensions
{
    public static string GetDeepLinkBaseUrl(this IConfiguration configuration) => configuration["DeepLinkBaseUrl"]!;
}