namespace SparkTrack.Authentication.WebAPI.Extensions;

using Core.Models;
using Core.Services.JwtAccessTokenGenerator;
using Core.Services.JwtRefreshTokenGenerator;
using Core.Services.JwtRefreshTokenValidator;
using Core.Services.RefreshTokensService;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtConfiguration(
        this IServiceCollection self,
        JwtConfiguration configuration
    ) => self.AddSingleton(configuration);

    public static IServiceCollection AddAccessTokenGenerator(this IServiceCollection self) =>
        self.AddScoped<IJwtAccessTokenGenerator, JwtAccessTokenGenerator>();

    public static IServiceCollection AddRefreshTokenGenerator(this IServiceCollection self) =>
        self.AddScoped<IJwtRefreshTokenGenerator, JwtRefreshTokenGenerator>();

    public static IServiceCollection AddRefreshTokenValidator(this IServiceCollection self) =>
        self.AddScoped<IJwtRefreshTokenValidator, JwtRefreshTokenValidator>();

    public static IServiceCollection AddRefreshTokenStorageConfiguration(
        this IServiceCollection self,
        RefreshTokensStorageConfiguration? configuration = null
    ) =>
        self.AddSingleton(
            configuration ?? new RefreshTokensStorageConfiguration { TokensLimitForUser = 5 }
        );

    public static IServiceCollection AddRefreshTokensService<TUserId>(
        this IServiceCollection self
    ) =>
        self.AddScoped<
            IRefreshTokensService<TUserId>,
            RefreshTokensService<TUserId>
        >();
}