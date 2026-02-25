namespace SparkTrack.DataAccess.EFCore.Repositories;

using System.Linq.Expressions;
using Core.Repositories;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Data.Entities;
using Extensions;
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
            PasswordHash = user.PasswordHash ?? throw new NullReferenceException(nameof(User.PasswordHash)),
            TelegramTag = user.TelegramTag
        };

        await dbContext.Users.AddAsync(userData);
        await dbContext.SaveChangesAsync();
    }

    public Task<User?> GetByEmailAsync(string email) => dbContext.Users.Where(it => it.Email == email)
        .Select(
            GetMapToUserExpression()
        )
        .FirstOrDefaultAsync();

    public async Task<User?> GetByTelegramTagAsync(string tag)
    {
        tag = tag.TrimStart('@');
        
        return await dbContext.Users.Where(it => it.TelegramTag == tag)
            .Select(GetMapToUserExpression())
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(User user)
    {
        var userData = await dbContext.Users.FindAsync(user.Id);

        if (userData == null) return;
        
        userData.Email = user.Email;
        userData.Name = user.Name;
        userData.Role = user.Role;
        userData.PasswordHash = user.PasswordHash ?? userData.PasswordHash;
        userData.TelegramTag = user.TelegramTag;

        await dbContext.SaveChangesAsync();
    }

    public Task<bool> UsersWithRoleExistsAsync(ERole role) => dbContext.Users.Where(it => it.Role == role).AnyAsync();

    public Task<IReadOnlyPagedData<User>> GetPageAsync(ERole role, PageQuery pageQuery) => dbContext.Users
        .AsNoTracking()
        .Where(
            it => it.Role == role
        )
        // TODO: Add filter
        .Where(it => it.ArchivedAt == null)
        .Select(
            GetMapToUserExpression()
        )
        .AsPaginated(pageQuery)
        .CollectAsync();

    public async Task DeleteAsync(Guid id)
    {
        var userData = await dbContext.Users.FindAsync(id);

        if (userData == null) return;

        dbContext.Users.Remove(userData);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task SetArchiveStatus(Guid id, bool isArchived, EArchiveSource? archiveSource = null)
    {
        var user = await dbContext.Users.FindAsync(id);

        if (user is null) return;

        user.ArchiveSource =
            isArchived ? archiveSource ?? throw new InvalidOperationException("Enter archive source") : null;
        
        user.ArchivedAt = isArchived ? DateTime.UtcNow : null;
        
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
            PasswordHash = it.PasswordHash,
            TelegramTag = it.TelegramTag,
            ArchivedAt = it.ArchivedAt,
            ArchiveSource = it.ArchiveSource
        };
    }
}