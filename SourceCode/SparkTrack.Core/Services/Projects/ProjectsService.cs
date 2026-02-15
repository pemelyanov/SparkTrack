namespace SparkTrack.Core.Services.Projects;

using Archive;
using Authorization;
using Extensions;
using Repositories;
using Shared.Data.Entities;
using Shared.Enums;
using Shared.Services.Projects;

internal class ProjectsService(
    IAuthorizationService authorizationService,
    IProjectsRepository projectsRepository,
    IProjectArchiveService projectArchiveService
)
    : IProjectsService
{
    public Task<IReadOnlyList<Project>> GetListAsync()
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        var employeeIdFilter = currentUser.GetEmployeeIdOrNull();

        return projectsRepository.GetListAsync(employeeIdFilter);
    }

    public Task AddAsync(Project project)
    {
        return projectsRepository.AddAsync(
            project with
            {
                Id = Guid.Empty
            }
        );
    }

    public Task EditAsync(Project project)
    {
        return projectsRepository.UpdateAsync(project);
    }

    public async Task DeleteAsync(Guid id, bool force)
    {
        if (force)
        {
            await projectsRepository.DeleteAsync(id);
            return;
        }

        await projectArchiveService.ArchiveAsync(id, EArchiveSource.User);
    }
}