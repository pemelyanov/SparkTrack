namespace SparkTrack.Core.Seeding.Development;

using Extensions;
using Microsoft.Extensions.Configuration;
using Services.Authorization;
using Services.Users;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Enums;
using Shared.Services.Features;
using Shared.Services.Projects;

public class FeaturesSeeder(
    IFeaturesService featuresService,
    IUsersService usersService,
    IProjectsService projectsService,
    IAuthorizationService authorizationService,
    IConfiguration configuration
) : DataSeederBase
{
    private const int FeaturesPerProject = 20;

    private static readonly IReadOnlyList<SubTaskEdit> s_availableTasks =
    [
        new()
        {
            Name = "Съемка",
            ExecutorEmployeeId = default,
            Deadline = DateTime.Now.AddDays(4).ToUniversalTime(),
            Cost = 2400
        },
        new()
        {
            Name = "Монтаж",
            ExecutorEmployeeId = default,
            Deadline = DateTime.Now.AddDays(7).ToUniversalTime(),
            Cost = 5000,
        },
        new()
        {
            Name = "Превью",
            ExecutorEmployeeId = default,
            Deadline = DateTime.Now.AddDays(8).ToUniversalTime(),
            Cost = 759
        }
    ];

    protected override async Task ProcessSeedAsync()
    {
        var god = await usersService.GetByEmailAsync(configuration.GetDefaultAdminModel().Email);
        await authorizationService.AuthorizeAsync(god?.Id);

        var projectsList = await projectsService.GetListAsync();
        var employeesList = (await usersService.GetPageAsync(ERole.Employee, PageQuery.All)).Items;

        var projectNumber = 0;
        foreach (var project in projectsList)
        {
            projectNumber++;
            var existingFeatures = (await featuresService.GetPageAsync(project.Id, true, null, null, PageQuery.None))
                .Total;

            if (existingFeatures > 0) continue;

            for (int i = 0; i < FeaturesPerProject; i++)
            {
                var feature = new FeatureEdit
                {
                    Name = $"Идея {i + 1}_{projectNumber}",
                    ProjectId = project.Id,
                    TasksList = s_availableTasks.Take(Random.Shared.Next(2, 4))
                        .Select(it => it with
                            {
                                ExecutorEmployeeId = employeesList[Random.Shared.Next(0, employeesList.Count)].Id
                            }
                        )
                        .ToArray(),
                    Description = "Нереальное описание",
                };

                await featuresService.AddAsync(feature);
            }
        }

        await authorizationService.AuthorizeAsync(null);
    }
}