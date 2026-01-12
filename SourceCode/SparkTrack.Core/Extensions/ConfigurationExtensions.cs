namespace SparkTrack.Core.Extensions;

using Microsoft.Extensions.Configuration;
using Shared.Data.Edit;

public static class ConfigurationExtensions
{
    public static UserEdit GetDefaultAdminModel(this IConfiguration configuration)
    {
        return configuration.GetSection("DefaultGod:UserEdit").Get<UserEdit>()!;
    }
    
    public static string GetDefaultAdminPassword(this IConfiguration configuration)
    {
        return configuration.GetSection("DefaultGod:Password").Get<string>()!;
    }
}