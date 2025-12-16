namespace SparkTrack.Authentication.WebAPI.Extensions;

using System.Text;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddDefaultJwtBearer(
        this AuthenticationBuilder self,
        JwtConfiguration configuration,
        Action<JwtBearerOptions>? config = null
    ) =>
        self.AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            //options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // укзывает, будет ли валидироваться издатель при валидации токена
                ValidateIssuer = true,
                // строка, представляющая издателя
                ValidIssuer = configuration.Issuer,
                // будет ли валидироваться потребитель токена
                ValidateAudience = true,
                // установка потребителя токена
                ValidAudience = configuration.Audience,
                // будет ли валидироваться время существования
                ValidateLifetime = true,
                // установка ключа безопасности
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(configuration.AccessTokenSecret)
                ),
                // валидация ключа безопасности
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };

            config?.Invoke(options);
        });
}