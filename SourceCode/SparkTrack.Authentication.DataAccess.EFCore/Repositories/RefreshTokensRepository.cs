namespace SparkTrack.Authentication.DataAccess.EFCore.Repositories;

using System.Linq.Expressions;
using Core.Data;
using Core.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;

internal class RefreshTokensRepository<TUserId>(RefreshTokenDbContext<TUserId> dbContext)
    : IRefreshTokensRepository<TUserId>
    where TUserId : notnull
{
    public async Task<IReadOnlyList<RefreshToken<TUserId>>> GetListAsync(TUserId userId) => await dbContext
        .RefreshTokens
        .AsNoTracking()
        .Where(it => Equals(userId, it.UserId))
        .Select(GetRefreshTokenExpression())
        .ToArrayAsync();

    public async Task<RefreshToken<TUserId>?> GetByHashOrTokenAsync(string hash, string token)
    {
        var tokenList = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(t => t.TokenHash == hash)
            .Select(GetRefreshTokenExpression())
            .ToArrayAsync();

        if (!tokenList.Any())
            return null;

        if (tokenList.Length > 1)
            return tokenList.FirstOrDefault(
                t => t.Token == token
            );

        return tokenList.First();
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> idsToRemoveList)
    {
        var ids = idsToRemoveList.ToList();
        if (!ids.Any())
            return;

        await dbContext.RefreshTokens
            .Where(t => ids.Contains(t.Id))
            .ExecuteDeleteAsync();
    }

    public async Task DeleteAllByUserIdAsync(TUserId userId)
    {
        await dbContext.RefreshTokens
            .Where(t => Equals(t.UserId, userId))
            .ExecuteDeleteAsync();
    }

    public async Task DeleteByHashOrTokenAsync(string hash, string token)
    {
        var tokenEntities = await dbContext.RefreshTokens
            .Where(t => t.TokenHash == hash)
            .ToListAsync();

        RefreshTokenData<TUserId>? tokenToRemove = null;

        if (tokenEntities.Count == 1)
        {
            tokenToRemove = tokenEntities.First();
        }
        else if (tokenEntities.Count > 1)
        {
            tokenToRemove = tokenEntities.FirstOrDefault(t => t.Token == token);
        }

        if (tokenToRemove != null)
        {
            dbContext.RefreshTokens.Remove(tokenToRemove);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<RefreshToken<TUserId>> AddAsync(RefreshToken<TUserId> refreshToken)
    {
        var tokenData = new RefreshTokenData<TUserId>
        {
            Id = refreshToken.Id,
            UserId = refreshToken.UserId,
            Token = refreshToken.Token,
            TokenHash = refreshToken.TokenHash,
            GenerationDate = refreshToken.GenerationDate
        };

        await dbContext.RefreshTokens.AddAsync(tokenData);
        await dbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task UpdateAsync(RefreshToken<TUserId> token)
    {
        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Id == token.Id);

        if (existingToken != null)
        {
            existingToken.Token = token.Token;
            existingToken.TokenHash = token.TokenHash;
            existingToken.GenerationDate = token.GenerationDate;

            await dbContext.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException($"Refresh token with id {token.Id} not found");
        }
    }

    private static Expression<Func<RefreshTokenData<TUserId>, RefreshToken<TUserId>>> GetRefreshTokenExpression()
    {
        return it => new RefreshToken<TUserId>
        {
            Id = it.Id,
            UserId = it.UserId,
            Token = it.Token,
            GenerationDate = it.GenerationDate,
            TokenHash = it.TokenHash
        };
    }
}