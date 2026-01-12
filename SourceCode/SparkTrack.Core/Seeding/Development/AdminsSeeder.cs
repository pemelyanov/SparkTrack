namespace SparkTrack.Core.Seeding.Development;

using Services.Authorization;
using Services.Users;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Enums;

public class AdminsSeeder(IUsersService usersService, IAuthorizationService authorizationService) : DataSeederBase
{
    private const int    AdminsQuantity  = 2;
    private const string DefaultPassword = "qwe123";
    
    protected override async Task ProcessSeedAsync()
    {
        var existingAdmins = (await usersService.GetPageAsync(ERole.Admin, PageQuery.None)).Total;
        
        if(existingAdmins > 0) return;

        for (int i = 0; i < AdminsQuantity; i++)
        {
            var adminEdit = new UserEdit
            {
                Email = $"admin{i+1}@admin",
                Name = $"Администратор {i+1}"
            };

            var generatedPassword = await authorizationService.RegisterAsync(adminEdit, ERole.Admin);

            var user = await usersService.GetByEmailAsync(adminEdit.Email);

            await authorizationService.AuthorizeAsync(user?.Id);
            await authorizationService.ChangePassword(generatedPassword, DefaultPassword);
            await authorizationService.AuthorizeAsync(null);
        }
    }
}