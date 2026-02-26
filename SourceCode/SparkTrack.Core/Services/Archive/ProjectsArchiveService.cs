namespace SparkTrack.Core.Services.Archive;

using Repositories;
using Shared.Data;
using Shared.Enums;
using Transactions;

public class ProjectsArchiveService(
    IProjectsRepository projectsRepository,
    IFeatureArchiveService featureArchiveService,
    ITransactionWrapper transactionWrapper,
    IFeaturesRepository featuresRepository
) : IProjectArchiveService
{
    public Task ArchiveAsync(Guid id, EArchiveSource source, bool executingInExternalTransaction = false)
    {
        if (executingInExternalTransaction) return ArchiveInternalAsync(id, source);

        return transactionWrapper.ExecuteInTransactionAsync(() => ArchiveInternalAsync(id, source));
    }

    private async Task ArchiveInternalAsync(Guid id, EArchiveSource source)
    {
        await projectsRepository.SetArchiveStatus(id, true, source);

        var features = await featuresRepository.GetPageAsync(id, true, null, null, null, null, null, PageQuery.All);

        foreach (var feature in features.Items.Where(it => it.ArchiveSource != EArchiveSource.User))
            await featureArchiveService.ArchiveAsync(feature.Id, EArchiveSource.Parent, true);
    }
}