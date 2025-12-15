namespace SparkTrack.Core.Services.Authorization;

using Shared.Data.Entities;

public interface IAuthorizationService
{
    User? CurrentUser { get; }

    Task AuthorizeAsync(Guid userId);
}