namespace SparkTrack.Core.Client.Services.Users;

using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;

public interface IUsersService
{
    Task<string> AddAsync(UserEdit user, ERole role);

    Task<IReadOnlyPagedData<User>> GetPageAsync(ERole role, PageQuery pageQuery);
    
    Task EditAsync(UserEdit userEdit);
}