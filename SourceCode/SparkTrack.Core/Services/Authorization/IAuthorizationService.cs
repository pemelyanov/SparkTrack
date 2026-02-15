namespace SparkTrack.Core.Services.Authorization;

using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;

public interface IAuthorizationService
{
    User? CurrentUser { get; }

    /// <summary>
    /// Принудительно устанавливает пользователя с указанным Id текущим
    /// </summary>
    Task AuthorizeAsync(Guid? userId);

    /// <summary>
    /// Создает пользователя с указанными данными
    /// </summary>
    /// <returns>Пароль пользователя</returns>
    Task<string> RegisterAsync(UserEdit userEdit, ERole role);
    
    Task<string> ResetPasswordAsync(Guid userId);

    Task<User?> LogInAsync(string email, string password);

    Task<bool> ChangePassword(string oldPassword, string newPassword);

    /// <summary>
    /// Создает дефолтный аккаунт бога, если его еще нет
    /// </summary>
    Task InvalidateDefaultGodAsync(UserEdit userData, string password);
}