namespace SparkTrack.API.Services.Features;

using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Services.Features;
using MappingExtensions;

internal class FeaturesService(Func<ClientWrapper<FeaturesClient>> featuresClientFactory) : IFeaturesService
{
    public async Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        bool showOnlyMine = true,
        FeatureFilterQuery? filterQuery = null,
        SortQuery? sortQuery = null,
        PageQuery? pageQuery = null
    )
    {
        using var clientWrapper = featuresClientFactory();
        var dto = await clientWrapper.Client.GetPageAsync(
            showOnlyMine: showOnlyMine,
            projectId: filterQuery?.ProjectId,
            showClosed: filterQuery?.ShowClosed,
            showCompleted: filterQuery?.ShowCompleted,
            startDate: filterQuery?.StartDate,
            endDate: filterQuery?.EndDate,
            sortField: sortQuery?.SortField,
            sortDescending: sortQuery?.SortDescending,
            page: pageQuery?.Page,
            itemsPerPage: pageQuery?.ItemsPerPage
        );

        var list = dto.Items.Select(it => it.ToDomain()).ToArray();

        return new ReadOnlyPagedData<Feature>(list, dto.Total);
    }

    public async Task<Feature?> GetAsync(int id)
    {
        using var clientWrapper = featuresClientFactory();

        var dto = await clientWrapper.Client.GetAsync(id);

        return dto.ToDomain();
    }

    public async Task<int> AddAsync(FeatureEdit feature)
    {
        using var clientWrapper = featuresClientFactory();

        return await clientWrapper.Client.AddAsync(feature.ToDTO());
    }

    public async Task EditAsync(FeatureEdit feature)
    {
        using var clientWrapper = featuresClientFactory();

        // TODO: Добавить обработку конфликта
        await clientWrapper.Client.EditAsync(feature.ToDTO());
    }

    public async Task DeleteAsync(int id, bool force)
    {
        using var clientWrapper = featuresClientFactory();

        await clientWrapper.Client.DeleteAsync(id, force);
    }

    public async Task SendOnPaymentAsync(IReadOnlyList<int> featuresIdList)
    {
        using var clientWrapper = featuresClientFactory();

        await clientWrapper.Client.SendOnPaymentAsync(featuresIdList);
    }
}