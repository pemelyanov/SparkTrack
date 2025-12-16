namespace SparkTrack.Core.Services.Authorization;

using Shared.Data.Entities;
using Shared.Enums;

public class FakeAuthService : IAuthorizationService
{
    public User? CurrentUser { get; } = new()
    {
        Id = Guid.Empty,
        Name = "asd",
        Role = ERole.God
    };

    public Task AuthorizeAsync(Guid userId) => throw new NotImplementedException();
}