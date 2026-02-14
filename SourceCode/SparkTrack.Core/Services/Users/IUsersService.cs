namespace SparkTrack.Core.Services.Users;

using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;

public interface IUsersService
{
    Task<IReadOnlyPagedData<User>> GetPageAsync(ERole role, PageQuery pageQuery);

    Task<User?> GetByEmailAsync(string email);

    Task EditAsync(UserEdit userEdit);
    
    Task DeleteAsync(Guid userId);
}