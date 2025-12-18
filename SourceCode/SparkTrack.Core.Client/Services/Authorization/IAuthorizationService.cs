namespace SparkTrack.Core.Client.Services.Authorization;

using Reactive;
using Shared.Data.Entities;

public interface IAuthorizationService
{
    IBehaviorObservable<User?> CurrentUser { get; }

    Task<bool> LogInAsync(string login, string password);

    Task<bool> TryAuthorizeExistingCredentials();

    Task LogOutAsync();
}