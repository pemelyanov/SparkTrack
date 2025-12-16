using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SparkTrack.Authentication.Core.Services.JwtRefreshTokenGenerator;

using Models;

public class JwtRefreshTokenGenerator(JwtConfiguration jwtConfiguration) : IJwtRefreshTokenGenerator
{
    public Task<string> GenerateRefreshTokenAsync()
    {
        return Task.Run(() =>
        {
            SecurityKey key = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(jwtConfiguration.RefreshTokenSecret)
            );
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwtConfiguration.Issuer,
                jwtConfiguration.Audience,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(jwtConfiguration.RefreshTokenExpirationMinutes),
                signingCredentials
            );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return encodedToken;
        });
    }
}