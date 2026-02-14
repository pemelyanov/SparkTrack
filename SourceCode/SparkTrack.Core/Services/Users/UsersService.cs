namespace SparkTrack.Core.Services.Users;

using Authorization;
using Exceptions;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;

internal class UsersService(IUsersRepository usersRepository, IAuthorizationService authorizationService)
    : IUsersService
{
    public Task<IReadOnlyPagedData<User>> GetPageAsync(ERole role, PageQuery pageQuery)
    {
        return usersRepository.GetPageAsync(role, pageQuery);
    }

    public Task<User?> GetByEmailAsync(string email) => usersRepository.GetByEmailAsync(email);

    public async Task EditAsync(UserEdit userEdit)
    {
        var existingUser = await usersRepository.GetAsync(userEdit.Id);

        if (existingUser is null) throw new NotFoundException($"User with id {userEdit.Id} not found");

        var allowedRole = existingUser.Role switch
        {
            ERole.Admin => ERole.God,
            ERole.Employee => ERole.God | ERole.Admin,
            _ => throw new NotSupportedException()
        };

        authorizationService.GetUserOrThrowIfNotInRole(allowedRole);

        var updatedUser = existingUser with
        {
            Email = userEdit.Email,
            Name = userEdit.Name,
            TelegramTag = userEdit.TelegramTag
        };

        await usersRepository.UpdateAsync(updatedUser);
    }
}