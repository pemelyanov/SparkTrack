namespace SparkTrack.Core.Repositories;

using Shared.Data.Entities;

public interface ISubTasksRepository
{
    Task<SubTask?> GetAsync(Guid id);
    
    Task<SubTask?> EditAsync(SubTask subTask);

    Task<IReadOnlyList<SubTask>> EditRangeAsync(IReadOnlyList<SubTask> subTasksList);
}