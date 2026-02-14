namespace SparkTrack.API.Services.Users;

using Core.Client.Services.Users;
using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using MappingExtensions;

public class UsersService(
    Func<ClientWrapper<AuthorizationClient>> authorizationClientFactory,
    Func<ClientWrapper<UsersClient>> usersClientFactory
) : IUsersService
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

    public async Task<IReadOnlyPagedData<User>> GetPageAsync(ERole role, PageQuery pageQuery)
    {
        using var clientWrapper = usersClientFactory();
        var task = role switch
        {
            ERole.Admin => clientWrapper.Client.GetAdminsListAsync(pageQuery.Page, pageQuery.ItemsPerPage),
            ERole.Employee => clientWrapper.Client.GetEmployeesListAsync(pageQuery.Page, pageQuery.ItemsPerPage),
            _ => throw new NotSupportedException(role.ToString())
        };

        var dto = await task;

        var list = dto.Items.Select(it => it.ToDomain()).ToArray();

        return new ReadOnlyPagedData<User>(list, dto.Total);
    }

    public async Task EditAsync(UserEdit userEdit)
    {
        using var clientWrapper = usersClientFactory();

        await clientWrapper.Client.EditAsync(userEdit.ToDTO());
    }

    public async Task DeleteAsync(Guid id)
    {
        using var clientWrapper = usersClientFactory();

        await clientWrapper.Client.DeleteAsync(id);
    }
}