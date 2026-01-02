namespace SparkTrack.API.Services.Projects;

using Core.Shared.Data.Entities;
using Core.Shared.Services.Projects;
using MappingExtensions;

internal class ProjectsService(Func<ClientWrapper<ProjectsClient>> projectsClientWrapperFactory) : IProjectsService
{
    public async Task<IReadOnlyList<Project>> GetListAsync()
    {
        using var clientWrapper = projectsClientWrapperFactory();

        var list = await clientWrapper.Client.GetListAsync();

        return list.Select(it => it.ToDomain()).ToArray();
    }

    public async Task AddAsync(Project project)
    {
        using var clientWrapper = projectsClientWrapperFactory();

        await clientWrapper.Client.AddAsync(project.ToDTO());
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}