namespace SparkTrack.API.Services.Authorization;

using Core.Client.Services.Authorization;
using Core.Client.Services.Configuration;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Data;
using MappingExtensions;
using Reactive;

internal class AuthorizationService(
    Func<ClientWrapper<AuthorizationClient>> authorizationClientFactory,
    IConfigurationService<TokensConfiguration> configurationService
) : IAuthorizationService
{
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

            m_currentUser.Value = new User
            {
                Id = authorizationDTO.UserId,
                Name = "", // TODO
                Email = "", // TODO
                Role = authorizationDTO.UserRole.Cast<ERole>(),
            };

            configurationService.UpdateConfig(
                new TokensConfiguration
                {
                    AccessToken = authorizationDTO.AccessToken,
                    RefreshToken = authorizationDTO.RefreshToken
                }
            );

            return true;
        }
        catch
        {
            // ignore
        }

        return false;
    }

    public Task<bool> TryAuthorizeExistingCredentials()
    {
        // TODO
        return Task.FromResult(false);
    }

    public async Task LogOutAsync()
    {
        using var authorizationClientWrapper = authorizationClientFactory();

        await authorizationClientWrapper.Client.LogOutAsync();

        configurationService.UpdateConfig(
            new TokensConfiguration
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty
            }
        );
    }
}