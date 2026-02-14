namespace SparkTrack.Core.Services.Projects;

using Authorization;
using Extensions;
using Repositories;
using Shared.Data.Entities;
using Shared.Services.Projects;

internal class ProjectsService(IAuthorizationService authorizationService, IProjectsRepository projectsRepository)
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

    public Task DeleteAsync(Guid id)
    {
        return projectsRepository.DeleteAsync(id);
    }
}