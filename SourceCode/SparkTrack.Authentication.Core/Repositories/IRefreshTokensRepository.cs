namespace SparkTrack.Authentication.Core.Repositories;

using Data;

public interface IRefreshTokensRepository<TUserId>
{
    Task<IReadOnlyList<RefreshToken<TUserId>>> GetListAsync(TUserId userId);
    
    Task<RefreshToken<TUserId>?> GetByHashOrTokenAsync(string hash, string token);
    
    Task DeleteRangeAsync(IEnumerable<Guid> idsToRemoveList);
    
    Task DeleteAllByUserIdAsync(TUserId userId);
    
    Task DeleteByHashOrTokenAsync(string hash, string token);

    Task<RefreshToken<TUserId>> AddAsync(RefreshToken<TUserId> userId);
    
    Task UpdateAsync(RefreshToken<TUserId> userId);
}