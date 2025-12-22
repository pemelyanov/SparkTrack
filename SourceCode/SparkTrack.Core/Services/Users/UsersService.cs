namespace SparkTrack.Core.Services.Users;

using Authorization;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Entities;
using Shared.Enums;

internal class UsersService(IUsersRepository usersRepository, IAuthorizationService authorizationService)
    : IUsersService
{
    public Task<IReadOnlyPagedData<User>> GetPageAsync(ERole role, PageQuery pageQuery)
    {
        if (role == ERole.Admin) authorizationService.GetUserOrThrowIfNotInRole(ERole.God);
        if (role == ERole.Employee) authorizationService.GetUserOrThrowIfNotInRole(ERole.Admin);

        return usersRepository.GetPageAsync(role, pageQuery);
    }
}