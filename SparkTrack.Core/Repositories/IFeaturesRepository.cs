namespace SparkTrack.Core.Repositories;

using Shared.Data;
using Shared.Data.Entities;

public interface IFeaturesRepository
{
    Task<IReadOnlyPagedData<Feature>> GetPageAsync(Guid? projectId, bool showCompleted, Guid? subTaskEmployeeId, PageQuery pageQuery);

    Task<Feature> GetAsync(int id);
}