namespace SparkTrack.API.Services.Users;

using Core.Client.Services.Users;
using Core.Shared.Data.Edit;
using Core.Shared.Enums;
using MappingExtensions;

public class UsersService(Func<ClientWrapper<AuthorizationClient>> authorizationClientFactory) : IUsersService
{
    public async Task<string> AddAsync(UserEdit user, ERole role)
    {
        using var clientWrapper = authorizationClientFactory.Invoke();
        var userDTO = user.ToDTO();
        var task = role switch
        {
            ERole.Admin => clientWrapper.Client.RegisterAdminAsync(userDTO),
            ERole.Employee => clientWrapper.Client.RegisterEmployeeAsync(userDTO),
            _ => throw new NotSupportedException()
        };

        return await task;
    }
}