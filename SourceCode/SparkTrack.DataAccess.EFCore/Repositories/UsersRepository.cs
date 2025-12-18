namespace SparkTrack.DataAccess.EFCore.Repositories;

using System.Linq.Expressions;
using Core.Repositories;
using Core.Shared.Data.Entities;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

internal class UsersRepository(SparkTrackDbContext dbContext) : IUsersRepository
{
    public Task<User?> GetAsync(Guid id) => dbContext.Users.Where(it => it.Id == id)
        .Select(
            GetMapToUserExpression()
        )
        .FirstOrDefaultAsync();

    public async Task AddAsync(User user)
    {
        var userData = new UserData
        {
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            PasswordHash = user.PasswordHash ?? throw new NullReferenceException(nameof(User.PasswordHash))
        };

        await dbContext.Users.AddAsync(userData);
        await dbContext.SaveChangesAsync();
    }

    public Task<User?> GetByEmailAsync(string email) => dbContext.Users.Where(it => it.Email == email)
        .Select(
            GetMapToUserExpression()
        )
        .FirstOrDefaultAsync();

    public async Task UpdateAsync(User user)
    {
        var userData = await dbContext.Users.FindAsync(user.Id);

        if (userData == null) return;
        
        userData.Email = user.Email;
        userData.Name = user.Name;
        userData.Role = user.Role;
        userData.PasswordHash = user.PasswordHash ?? userData.PasswordHash;

        await dbContext.SaveChangesAsync();
    }

    private static Expression<Func<UserData, User>> GetMapToUserExpression()
    {
        return it => new User
        {
            Id = it.Id,
            Email = it.Email,
            Name = it.Name,
            Role = it.Role,
            PasswordHash = it.PasswordHash
        };
    }
}