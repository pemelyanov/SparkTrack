namespace SparkTrack.API.Services.Authorization;

using Core.Client.Services.Authorization;
using Core.Client.Services.Configuration;
using Core.Shared.Data.Entities;
using Data;
using MappingExtensions;
using NLog;
using Reactive;

internal class AuthorizationService(
    Func<ClientWrapper<AuthorizationClient>> authorizationClientFactory,
    Func<ClientWrapper<ProfileClient>> profileClientFactory,
    IConfigurationService<TokensConfiguration> configurationService
) : IAuthorizationService
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    private readonly BehaviorObservableSubject<User?> m_currentUser = new(null);

    public IBehaviorObservable<User?> CurrentUser => m_currentUser;

    public async Task<bool> LogInAsync(string login, string password)
    {
        using var authorizationClientWrapper = authorizationClientFactory();

        try
        {
            var authorizationDTO = await authorizationClientWrapper.Client.LogInAsync(
                new LogInDTO
                {
                    Email = login,
                    Password = password
                }
            );

            configurationService.UpdateConfig(
                new TokensConfiguration
                {
                    AccessToken = authorizationDTO.AccessToken,
                    RefreshToken = authorizationDTO.RefreshToken
                }
            );
            
            m_currentUser.Value = await GetCurrentProfileAsync();

            return true;
        }
        catch
        {
            // ignore
        }

        return false;
    }

    public async Task<bool> TryAuthorizeExistingCredentials()
    {
        try
        {
            var config = configurationService.Config;

            if (string.IsNullOrEmpty(config.AccessToken) || string.IsNullOrEmpty(config.RefreshToken)) return false;
            
            m_currentUser.Value = await GetCurrentProfileAsync();

            return true;
        }
        catch (Exception e)
        {
            s_logger.Warn(e);
        }

        return false;
    }

    public async Task LogOutAsync()
    {
        using var authorizationClientWrapper = authorizationClientFactory();

        try
        {
            await authorizationClientWrapper.Client.LogOutAsync(configurationService.Config.RefreshToken);
        }
        catch (Exception e)
        {
            s_logger.Warn(e);
        }

        configurationService.UpdateConfig(
            new TokensConfiguration
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty
            }
        );

        m_currentUser.Value = null;
    }

    private async Task<User> GetCurrentProfileAsync()
    {
        using var profileClientWrapper = profileClientFactory();

        var user = await profileClientWrapper.Client.GetAsync();

        return user.ToDomain();
    }
}