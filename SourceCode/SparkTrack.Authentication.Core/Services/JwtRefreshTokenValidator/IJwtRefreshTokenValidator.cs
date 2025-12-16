namespace SparkTrack.Authentication.Core.Services.JwtRefreshTokenValidator;

public interface IJwtRefreshTokenValidator
{
    Task<bool> ValidateAsync(string refreshToken);
}