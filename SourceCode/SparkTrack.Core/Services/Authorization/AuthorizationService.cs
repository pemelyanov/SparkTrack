namespace SparkTrack.Core.Services.Authorization;

using Exceptions;
using Extensions;
using PasswordGenerator;
using PasswordHasher;
using Repositories;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;

public class AuthorizationService(
    IUsersRepository usersRepository,
    IPasswordHasher passwordHasher
)
    : IAuthorizationService
{
    private const int PasswordLength = 8;
    
    public User? CurrentUser { get; private set; }

    public async Task AuthorizeAsync(Guid? userId)
    {
        if (userId is null)
        {
            CurrentUser = null;
            return;
        }

        CurrentUser = await usersRepository.GetAsync(userId.Value);
    }

    public async Task<string> RegisterAsync(UserEdit userEdit, ERole role)
    {
        var password = new Password().LengthRequired(PasswordLength).Next();

        var user = new User
        {
            Email = userEdit.Email,
            Name = userEdit.Name,
            Role = role,
            PasswordHash = await passwordHasher.HashAsync(password),
            TelegramTag = userEdit.TelegramTag
        };

        await usersRepository.AddAsync(user);

        return password;
    }

    public async Task<string> ResetPasswordAsync(Guid userId)
    {
        var existingUser = await usersRepository.GetAsync(userId);

        if (existingUser is null) throw new NotFoundException($"User with id {userId} not found");

        var allowedRole = existingUser.Role switch
        {
            ERole.Admin => ERole.God,
            ERole.Employee => ERole.God | ERole.Admin,
            _ => throw new NotSupportedException()
        };

        this.GetUserOrThrowIfNotInRole(allowedRole);
        
        var password = new Password().LengthRequired(PasswordLength).Next();

        var updatedUser = existingUser with
        {
            PasswordHash = await passwordHasher.HashAsync(password)
        };

        await usersRepository.UpdateAsync(updatedUser);

        return password;
    }

    public async Task<User?> LogInAsync(string email, string password)
    {
        var user = await usersRepository.GetByEmailAsync(email);

        if (user?.PasswordHash is null || !await passwordHasher.VerifyAsync(password, user.PasswordHash)) return null;

        return user;
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

    public async Task InvalidateDefaultGodAsync(UserEdit userData, string password)
    {
        if (await usersRepository.UsersWithRoleExistsAsync(ERole.God)) return;

        var god = new User
        {
            Email = userData.Email,
            Name = userData.Name,
            Role = ERole.God,
            PasswordHash = await passwordHasher.HashAsync(password),
            TelegramTag = userData.TelegramTag
        };

        await usersRepository.AddAsync(god);
    }
}