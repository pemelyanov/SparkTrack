namespace SparkTrack.Core.Seeding;

using Extensions;
using Microsoft.Extensions.Configuration;
using Services.Authorization;

internal class DefaultGodSeeder(IConfiguration configuration, IAuthorizationService authorizationService) : DataSeederBase
{
    protected override async Task ProcessSeedAsync()
    {
        var defaultGod = configuration.GetDefaultAdminModel();
        var defaultGodPassword = configuration.GetDefaultAdminPassword();

        await authorizationService
            .InvalidateDefaultGodAsync(defaultGod, defaultGodPassword);
    }
}