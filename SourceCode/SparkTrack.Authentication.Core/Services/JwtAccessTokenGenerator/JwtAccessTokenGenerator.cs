using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SparkTrack.Authentication.Core.Services.JwtAccessTokenGenerator;

using Models;

public class JwtAccessTokenGenerator(JwtConfiguration jwtConfiguration) : IJwtAccessTokenGenerator
{
    public Task<string> GenerateAccessTokenAsync(List<Claim> claimList)
    {
        return Task.Run(() =>
        {
            SecurityKey key = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(jwtConfiguration.AccessTokenSecret)
            );

            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwtConfiguration.Issuer,
                jwtConfiguration.Audience,
                claimList,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(jwtConfiguration.AccessTokenExpirationMinutes),
                signingCredentials
            );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return encodedToken;
        });
    }
}