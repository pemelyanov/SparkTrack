namespace SparkTrack.Core.Client.Services.Authorization;

using Reactive;
using Shared.Data.Entities;
using Shared.Enums;

internal class FakeAuthorizationService : IAuthorizationService
{
    private readonly BehaviorObservableSubject<User?> m_currentUser = new(null);

    public IBehaviorObservable<User?> CurrentUser => m_currentUser;

    public Task<bool> LogInAsync(string login, string password)
    {
        m_currentUser.Value = new User
        {
            Id = Guid.Empty,
            Name = "Самбади",
            Role = ERole.Admin
        };

        return Task.FromResult(true);
    }

    public Task LogOut()
    {
        m_currentUser.Value = null;
        
        return Task.CompletedTask;
    }

    public Task RefreshCredentialsAsync() => Task.CompletedTask;
}