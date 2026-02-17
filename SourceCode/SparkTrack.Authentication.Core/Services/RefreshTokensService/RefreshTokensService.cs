using System.Security.Cryptography;
using System.Text;

namespace SparkTrack.Authentication.Core.Services.RefreshTokensService;

using Data;
using Models;
using Repositories;

public class RefreshTokensService<TUserId>(
    IRefreshTokensRepository<TUserId> refreshTokensRepository,
    RefreshTokensStorageConfiguration tokesStorageConfiguration
)
    : IRefreshTokensService<TUserId>
{
    public async Task<RefreshToken<TUserId>> AddTokenAsync(string token, TUserId userId)
    {
        var existingTokens = await refreshTokensRepository.GetListAsync(userId);

        var tokensLimit = tokesStorageConfiguration.TokensLimitForUser - 1;

        if (existingTokens.Count > tokensLimit)
        {
            var tokensToDelete = existingTokens.Skip(tokensLimit).Select(it => it.Id).ToArray();

            await refreshTokensRepository.DeleteRangeAsync(tokensToDelete);
        }

        var tokenHash = GetTokenHash(token);

        var refreshToken = new RefreshToken<TUserId>
        {
            UserId = userId,
            Token = token,
            TokenHash = tokenHash
        };

        var updatedToken = await refreshTokensRepository.AddAsync(refreshToken);

        return updatedToken;
    }

    public Task DeleteAllTokensByUserId(TUserId userId)
    {
        return refreshTokensRepository.DeleteAllByUserIdAsync(userId);
    }

    public Task DeleteTokenAsync(string token)
    {
        var hash = GetTokenHash(token);
        
        return refreshTokensRepository.DeleteByHashOrTokenAsync(hash, token);
    }

    public async Task<RefreshToken<TUserId>?> GetTokenAsync(string token)
    {
        var refreshToken = await GetTokenDataAsync(token);

        if (refreshToken == null)
            return null;

        return refreshToken;
    }

    public async Task<RefreshToken<TUserId>?> UpdateTokenAsync(
        string oldToken,
        string newToken
    )
    {
        var refreshToken = await GetTokenDataAsync(oldToken);

        if (refreshToken == null)
            return null;

        refreshToken.Token = newToken;
        refreshToken.TokenHash = GetTokenHash(newToken);
        refreshToken.GenerationDate = DateTime.UtcNow;

        await refreshTokensRepository.UpdateAsync(refreshToken);

        return refreshToken;
    }
    
    private string GetTokenHash(string token)
    {
        return Convert.ToHexString(MD5.HashData(Encoding.ASCII.GetBytes(token)));
    }

    private Task<RefreshToken<TUserId>?> GetTokenDataAsync(string token)
    {
        var tokenHash = GetTokenHash(token);

        return refreshTokensRepository.GetByHashOrTokenAsync(tokenHash, token);
    }
}