namespace SparkTrack.Core.Services.Users;

using Repositories;
using Shared.Data;
using Shared.Data.Entities;
using Shared.Enums;

internal class UsersService(IUsersRepository usersRepository)
    : IUsersService
{
    public Task<IReadOnlyPagedData<User>> GetPageAsync(ERole role, PageQuery pageQuery)
    {
        return usersRepository.GetPageAsync(role, pageQuery);
    }

    public Task<User?> GetByEmailAsync(string email) => usersRepository.GetByEmailAsync(email);
}