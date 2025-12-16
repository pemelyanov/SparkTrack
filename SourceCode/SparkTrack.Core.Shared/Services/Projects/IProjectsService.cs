namespace SparkTrack.Core.Shared.Services.Projects;

using Data.Entities;

public interface IProjectsService
{
    /// <summary>
    /// Получает список проектов.
    /// </summary>
    Task<IReadOnlyList<Project>> GetListAsync();

    Task AddAsync(Project project);

    Task DeleteAsync(Guid id);
}