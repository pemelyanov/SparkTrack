namespace SparkTrack.Core.Services.Users;

using Shared.Data;
using Shared.Data.Entities;
using Shared.Enums;

public interface IUsersService
{
    Task<IReadOnlyPagedData<User>> GetPageAsync(ERole role, PageQuery pageQuery);
}