using System.Security.Claims;

namespace SparkTrack.Authentication.Core.Services.JwtAccessTokenGenerator;

public interface IJwtAccessTokenGenerator
{
    Task<string> GenerateAccessTokenAsync(List<Claim> claimList);
}