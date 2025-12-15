namespace SparkTrack.Core.Services.Features;

using Shared.Data;
using Shared.Data.Entities;

public interface IFeaturesService
{
    Task<IReadOnlyPagedData<Feature>> GetPageAsync(Guid? projectId, bool showCompleted, PageQuery pageQuery);

    Task<Feature> GetAsync(int id);
}