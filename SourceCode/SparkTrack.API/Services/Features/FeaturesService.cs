namespace SparkTrack.API.Services.Features;

using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Services.Features;
using MappingExtensions;

internal class FeaturesService(Func<ClientWrapper<FeaturesClient>> featuresClientFactory) : IFeaturesService
{
    public async Task<IReadOnlyPagedData<Feature>> GetPageAsync(Guid? projectId, bool showCompleted, PageQuery pageQuery)
    {
        using var clientWrapper = featuresClientFactory();
        var dto = await clientWrapper.Client.GetPageAsync(projectId, showCompleted, pageQuery.Page, pageQuery.ItemsPerPage);

        var list = dto.Items.Select(it => it.ToDomain()).ToArray();

        return new ReadOnlyPagedData<Feature>(list, dto.Total);
    }

    public Task<Feature?> GetAsync(int id) => throw new NotImplementedException();

    public Task AddAsync(FeatureEdit feature) => throw new NotImplementedException();

    public Task DeleteAsync(int id) => throw new NotImplementedException();
}