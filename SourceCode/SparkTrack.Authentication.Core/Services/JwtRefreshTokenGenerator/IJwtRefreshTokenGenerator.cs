namespace SparkTrack.Authentication.Core.Services.JwtRefreshTokenGenerator;

public interface IJwtRefreshTokenGenerator
{
    Task<string> GenerateRefreshTokenAsync();
}