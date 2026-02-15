namespace SparkTrack.Core.Services.Archive;

using Repositories;
using Shared.Enums;

public class FeatureArchiveService(IFeaturesRepository featuresRepository) : IFeatureArchiveService
{
    public Task ArchiveAsync(int id, EArchiveSource source, bool executingInExternalTransaction = false) =>
        featuresRepository.SetArchiveStatus(id, true, source);
}