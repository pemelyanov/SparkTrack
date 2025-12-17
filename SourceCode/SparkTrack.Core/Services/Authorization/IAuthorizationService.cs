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
    Task AuthorizeAsync(Guid userId);

    /// <summary>
    /// Создает пользователя с указанными данными
    /// </summary>
    /// <returns>Пароль пользователя</returns>
    Task<string> RegisterAsync(UserEdit userEdit, ERole role);

    Task<bool> LoginAsync(string email, string password);

    Task<bool> ChangePassword(string oldPassword, string newPassword);
}