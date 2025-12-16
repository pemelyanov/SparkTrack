using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SparkTrack.Authentication.Core.Services.JwtRefreshTokenValidator;

using Models;

public class JwtRefreshTokenValidator(JwtConfiguration jwtConfiguration) : IJwtRefreshTokenValidator
{
    public Task<bool> ValidateAsync(string refreshToken)
    {
        return Task.Run(() =>
        {
            var validationParameters = new TokenValidationParameters
            {
                // укзывает, будет ли валидироваться издатель при валидации токена
                ValidateIssuer = true,
                // строка, представляющая издателя
                ValidIssuer = jwtConfiguration.Issuer,
                // будет ли валидироваться потребитель токена
                ValidateAudience = true,
                // установка потребителя токена
                ValidAudience = jwtConfiguration.Audience,
                // будет ли валидироваться время существования
                ValidateLifetime = true,
                // установка ключа безопасности
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(jwtConfiguration.RefreshTokenSecret)
                ),
                // валидация ключа безопасности
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                jwtSecurityTokenHandler.ValidateToken(
                    refreshToken,
                    validationParameters,
                    out SecurityToken _
                );
                return true;
            }
            catch
            {
                return false;
            }
        });
    }
}