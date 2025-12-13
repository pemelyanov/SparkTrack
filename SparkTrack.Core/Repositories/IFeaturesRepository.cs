namespace SparkTrack.Core.Repositories;

using Data;
using Data.Entities;

public interface IFeaturesRepository
{
    Task<IReadOnlyPagedData<Feature>> GetPageAsync(Guid? projectId, bool showCompleted, Guid? subTaskEmployeeId, PageQuery pageQuery);

    Task<Feature> GetAsync(int id);
}