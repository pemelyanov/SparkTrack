namespace SparkTrack.Authentication.Core.Services.RefreshTokensService;

using Data;

public interface IRefreshTokensService<TUserId>
{
    Task<RefreshToken<TUserId>> AddTokenAsync(string token, TUserId userId);

    Task DeleteTokenAsync(string token);

    Task DeleteAllTokensByUserId(TUserId userId);

    Task<RefreshToken<TUserId>?> GetTokenAsync(string token);

    Task<RefreshToken<TUserId>?> UpdateTokenAsync(string oldToken, string newToken);
}