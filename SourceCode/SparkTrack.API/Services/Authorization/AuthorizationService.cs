namespace SparkTrack.API.Services.Authorization;

using Core.Client.Events;
using Core.Client.Services.Authorization;
using Core.Client.Services.Configuration;
using Core.Shared.Data.Entities;
using Core.Shared.Eventing;
using Core.Shared.Extensions;
using Data;
using Delegates;
using MappingExtensions;
using NLog;
using Reactive;
using System.Net;

internal class AuthorizationService(
    ClientFactory<AuthorizationClient> authorizationClientFactory,
    ClientFactory<ProfileClient> profileClientFactory,
    IConfigurationService<TokensConfiguration> configurationService,
    IEventEmitter eventEmitter
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

            await eventEmitter.RaiseAsync<LogoutEvent>();
            
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

    public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        using var authorizationClientWrapper = authorizationClientFactory();

        try
        {
            await authorizationClientWrapper.Client.ChangePasswordAsync(
                new ChangePasswordDTO
                {
                    NewPassword = newPassword,
                    OldPassword = oldPassword
                }
            );

            return true;
        }
        catch (ApiException e) when (e.StatusCode == (int)HttpStatusCode.BadRequest)
        {
            return false;
        }
    }

    public async Task<string> ResetPasswordAsync(Guid userId)
    {
        using var authorizationClientWrapper = authorizationClientFactory();

        return await authorizationClientWrapper.Client.ResetPasswordAsync(userId);
    }

    private async Task<User> GetCurrentProfileAsync()
    {
        using var profileClientWrapper = profileClientFactory();

        var user = await profileClientWrapper.Client.GetAsync();

        return user.ToDomain();
    }
}