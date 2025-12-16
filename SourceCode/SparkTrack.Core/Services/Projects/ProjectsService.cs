namespace SparkTrack.Core.Services.Projects;

using Authorization;
using Extensions;
using Repositories;
using Shared.Data.Entities;
using Shared.Enums;
using Shared.Services.Projects;

internal class ProjectsService(IAuthorizationService authorizationService, IProjectsRepository projectsRepository) : IProjectsService
{
    public Task<IReadOnlyList<Project>> GetListAsync()
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        var employeeIdFilter = currentUser.GetEmployeeIdOrNull();

        return projectsRepository.GetListAsync(employeeIdFilter);
    }

    public Task AddAsync(Project project)
    {
        authorizationService.GetUserOrThrowIfNotInRole(ERole.God);

        return projectsRepository.AddAsync(project with { Id = Guid.Empty });
    }

    public Task DeleteAsync(Guid id)
    {
        authorizationService.GetUserOrThrowIfNotInRole(ERole.God);

        return projectsRepository.DeleteAsync(id);
    }
}