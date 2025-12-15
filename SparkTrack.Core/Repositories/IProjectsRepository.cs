namespace SparkTrack.Core.Repositories;

using Shared.Data.Entities;

public interface IProjectsRepository
{
    /// <summary>
    /// Получает список проектов.
    /// </summary>
    /// <param name="userId">Если указано, то возвращает список проектов, в которых участвует пользователь с указанным Id</param>
    /// <returns></returns>
    Task<IReadOnlyList<Project>> GetListAsync(Guid? userId = null);

    Task AddAsync(Project project);

    Task DeleteAsync(Guid id);
}