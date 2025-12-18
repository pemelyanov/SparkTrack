namespace SparkTrack.Core.Repositories;

using Shared.Data.Entities;
using Shared.Enums;

public interface IUsersRepository
{
    Task<User?> GetAsync(Guid id);
    
    Task AddAsync(User user);
    
    Task<User?> GetByEmailAsync(string email);

    Task UpdateAsync(User user);

    Task<bool> UsersWithRoleExistsAsync(ERole role);
}