namespace SparkTrack.Core.Services.Authorization;

using Data.Entities;

public interface IAuthorizationService
{
    User? CurrentUser { get; }

    Task AuthorizeAsync(Guid userId);
}