namespace SparkTrack.Core.Client.Services.Users;

using Shared.Data.Edit;
using Shared.Enums;

public interface IUsersService
{
    Task<string> AddAsync(UserEdit user, ERole role);
}