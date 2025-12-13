namespace SparkTrack.Core.Services.Features;

using Data;
using Data.Entities;

public interface IFeaturesService
{
    Task<IReadOnlyPagedData<Feature>> GetPageAsync(Guid? projectId, bool showCompleted, PageQuery pageQuery);

    Task<Feature> GetAsync(int id);
}