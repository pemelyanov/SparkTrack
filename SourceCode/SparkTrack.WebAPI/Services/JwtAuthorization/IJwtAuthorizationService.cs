namespace SparkTrack.WebAPI.Services.JwtAuthorization;

using Core.Shared.Enums;
using DTO;
using DTO.Edit;

public interface IJwtAuthorizationService
{
    Task<AuthorizationDTO?> LogInAsync(string email, string password);
    
    Task<AuthorizationDTO?> RefreshTokensAsync(string refreshToken);
    
    Task LogOutAsync(string refreshToken);

    Task LogOutAllAsync(Guid userId);
    
    /// <summary>
    /// Создает пользователя с указанными данными
    /// </summary>
    /// <returns>Пароль пользователя</returns>
    Task<string> RegisterAsync(UserEditDTO userEdit, ERole role);
    
    Task<bool> ChangePassword(string oldPassword, string newPassword);
}