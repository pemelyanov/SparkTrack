namespace SparkTrack.Core.Services.Authorization;

using PasswordGenerator;
using PasswordHasher;
using Repositories;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;

public class AuthorizationService(IUsersRepository usersRepository, IPasswordHasher passwordHasher)
    : IAuthorizationService
{
    public User? CurrentUser { get; private set; }

    public async Task AuthorizeAsync(Guid userId)
    {
        CurrentUser = await usersRepository.GetAsync(userId);
    }

    public async Task<string> RegisterAsync(UserEdit userEdit, ERole role)
    {
        var password = new Password().LengthRequired(8).Next();

        var user = new User
        {
            Email = userEdit.Email,
            Name = userEdit.Name,
            Role = role,
            PasswordHash = await passwordHasher.HashAsync(password)
        };

        await usersRepository.AddAsync(user);

        return password;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await usersRepository.GetByEmailAsync(email);

        if (user?.PasswordHash is null) return false;

        return await passwordHasher.VerifyAsync(password, user.PasswordHash);
    }

    public async Task<bool> ChangePassword(string oldPassword, string newPassword)
    {
        if (CurrentUser?.PasswordHash is null) return false;

        if (!await passwordHasher.VerifyAsync(oldPassword, CurrentUser.PasswordHash)) return false;

        await usersRepository.UpdateAsync(
            CurrentUser with
            {
                PasswordHash = await passwordHasher.HashAsync(newPassword)
            }
        );

        return true;
    }
}