namespace SparkTrack.Core.Repositories;

using Data.Entities;
using Shared.Data;
using Shared.Data.Entities;

public interface ISubTasksRepository
{
    Task<SubTask?> GetAsync(Guid id);

    Task<IReadOnlyList<EditableEntityIdentity>> GetIdentityListByFeatureIdListAsync(IReadOnlyList<int> featuresIdList);
    
    Task<Feature?> GetParentFeatureAsync(Guid id);

    Task<IReadOnlyList<SubTaskWithPayments>> GetListAsync(IReadOnlyList<Guid> idList);
    
    Task<SubTask?> EditAsync(SubTask subTask);

    Task<IReadOnlyList<SubTask>> EditRangeAsync(IReadOnlyList<SubTask> subTasksList);
}