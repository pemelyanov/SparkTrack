namespace SparkTrack.Core.Seeding.Development;

using Services.Authorization;
using Services.Users;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Enums;

public class EmployeesSeeder(IUsersService usersService, IAuthorizationService authorizationService) : DataSeederBase
{
    private const int    EmployeesQuantity  = 9;
    private const string DefaultPassword = "qwe123";

    protected override async Task ProcessSeedAsync()
    {
        var existingAdmins = (await usersService.GetPageAsync(ERole.Employee, PageQuery.None)).Total;
        
        if(existingAdmins > 0) return;

        for (int i = 0; i < EmployeesQuantity; i++)
        {
            var adminEdit = new UserEdit
            {
                Email = $"emp{i+1}@emp",
                Name = $"Сотрудник {i+1}"
            };

            var generatedPassword = await authorizationService.RegisterAsync(adminEdit, ERole.Employee);

            var user = await usersService.GetByEmailAsync(adminEdit.Email);

            await authorizationService.AuthorizeAsync(user?.Id);
            await authorizationService.ChangePassword(generatedPassword, DefaultPassword);
            await authorizationService.AuthorizeAsync(null);
        }
    }
}