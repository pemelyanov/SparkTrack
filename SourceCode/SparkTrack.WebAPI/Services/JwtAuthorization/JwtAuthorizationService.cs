namespace SparkTrack.WebAPI.Services.JwtAuthorization;

using System.Security.Claims;
using Authentication.Core.Services.JwtAccessTokenGenerator;
using Authentication.Core.Services.JwtRefreshTokenGenerator;
using Authentication.Core.Services.JwtRefreshTokenValidator;
using Authentication.Core.Services.RefreshTokensService;
using Constants;
using Core.Repositories;
using Core.Services.Authorization;
using Core.Shared.Enums;
using DTO;
using DTO.Edit;
using MappingExtensions;

internal class JwtAuthorizationService(
    IAuthorizationService authorizeService,
    IRefreshTokensService<Guid> refreshTokensService,
    IJwtAccessTokenGenerator jwtAccessTokenGenerator,
    IJwtRefreshTokenGenerator jwtRefreshTokenGenerator,
    IJwtRefreshTokenValidator jwtRefreshTokenValidator,
    IUsersRepository usersRepository
) : IJwtAuthorizationService
{
    public async Task<AuthorizationDTO?> LogInAsync(string email, string password)
    {
        var user = await authorizeService.LogInAsync(email, password);

        if (user == null)
            return null;

        var accessToken = await jwtAccessTokenGenerator.GenerateAccessTokenAsync(
            GetUserClaimList(user.Id, user.Role.ToString())
        );

        var refreshToken = await jwtRefreshTokenGenerator.GenerateRefreshTokenAsync();

        await refreshTokensService.AddTokenAsync(refreshToken, user.Id);

        return new AuthorizationDTO
        {
            RefreshToken = refreshToken,
            AccessToken = accessToken,
            UserId = user.Id,
            UserRole = user.Role
        };
    }

    public async Task<AuthorizationDTO?> RefreshTokensAsync(string refreshToken)
    {
        var isTokenValid = await jwtRefreshTokenValidator.ValidateAsync(refreshToken);

        if (!isTokenValid)
            return null;

        var token = await refreshTokensService.GetTokenAsync(refreshToken);

        if (token is null)
            return null;

        var user = await usersRepository.GetAsync(token.UserId);

        if (user is null)
            return null;

        var newRefreshToken = await jwtRefreshTokenGenerator.GenerateRefreshTokenAsync();

        await refreshTokensService.UpdateTokenAsync(refreshToken, newRefreshToken);

        var newAccessToken = await jwtAccessTokenGenerator.GenerateAccessTokenAsync(
            GetUserClaimList(user.Id, user.Role.ToString())
        );

        return new AuthorizationDTO
        {
            RefreshToken = newRefreshToken,
            AccessToken = newAccessToken,
            UserId = user.Id,
            UserRole = user.Role
        };
    }

    public async Task LogOutAllAsync(Guid userId)
    {
        await refreshTokensService.DeleteAllTokensByUserId(userId);
    }

    public async Task LogOutAsync(string refreshToken)
    {
        await refreshTokensService.DeleteTokenAsync(refreshToken);
    }

    public Task<string> RegisterAsync(UserEditDTO userEdit, ERole role) =>
        authorizeService.RegisterAsync(userEdit.ToDomain(), role);

    public Task<bool> ChangePassword(string oldPassword, string newPassword) =>
        authorizeService.ChangePassword(oldPassword, newPassword);

    private List<Claim> GetUserClaimList(Guid userId, string roleName)
    {
        return
        [
            new(CommonClaims.UserId, userId.ToString()),
            new(ClaimsIdentity.DefaultRoleClaimType, roleName),
        ];
    }
}