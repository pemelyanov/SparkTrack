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

    public async Task<int> AddAsync(FeatureEdit feature)
    {
        using var clientWrapper = featuresClientFactory();

        return await clientWrapper.Client.AddAsync(feature.ToDTO());
    }

    public async Task EditAsync(FeatureEdit feature)
    {
        using var clientWrapper = featuresClientFactory();

        await clientWrapper.Client.EditAsync(feature.ToDTO());
    }

    public Task DeleteAsync(int id) => throw new NotImplementedException();
}