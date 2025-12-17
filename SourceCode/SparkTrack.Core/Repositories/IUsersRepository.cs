namespace SparkTrack.Core.Repositories;

using Shared.Data.Entities;

public interface IUsersRepository
{
    Task<User?> GetAsync(Guid id);
    
    Task AddAsync(User user);
    
    Task<User?> GetByEmailAsync(string email);

    Task UpdateAsync(User user);
}