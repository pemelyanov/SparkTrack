namespace SparkTrack.Telegram.Core.Extensions;

using Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static string GetDeepLinkBaseUrl(this IConfiguration configuration) => configuration["DeepLinkBaseUrl"]!;
}