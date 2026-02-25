namespace SparkTrack.Core.Services.Archive;

using Events;
using Exceptions;
using Repositories;
using Shared.Enums;
using Shared.Eventing;

public class FeatureArchiveService(IFeaturesRepository featuresRepository, IEventEmitter eventEmitter) : IFeatureArchiveService
{
    public async Task ArchiveAsync(int id, EArchiveSource source, bool executingInExternalTransaction = false)
    {
        var feature = await featuresRepository.GetAsync(id, null);

        if (feature is null) throw new NotFoundException();
        
        await featuresRepository.SetArchiveStatus(id, true, source);

        await eventEmitter.RaiseAsync(new FeatureDeletedEvent(feature, source));
    }
}