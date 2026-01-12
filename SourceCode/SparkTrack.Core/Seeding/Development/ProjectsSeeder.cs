namespace SparkTrack.Core.Seeding.Development;

using Extensions;
using Microsoft.Extensions.Configuration;
using Services.Authorization;
using Services.Users;
using Shared.Data.Entities;
using Shared.Services.Projects;

public class ProjectsSeeder(
    IProjectsService projectsService,
    IAuthorizationService authorizationService,
    IUsersService usersService,
    IConfiguration configuration
) : DataSeederBase
{
    private const int ProjectsQuantity = 2;

    protected override async Task ProcessSeedAsync()
    {
        var god = await usersService.GetByEmailAsync(configuration.GetDefaultAdminModel().Email);
        await authorizationService.AuthorizeAsync(god?.Id);

        var existingProjects = await projectsService.GetListAsync();

        if (existingProjects.Count > 0) return;

        for (int i = 0; i < ProjectsQuantity; i++)
        {
            var project = new Project
            {
                Name = $"Канал {i + 1}",
            };

            await projectsService.AddAsync(project);
        }

        await authorizationService.AuthorizeAsync(null);
    }
}