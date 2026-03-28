using SparkTrack.Core.Events;
using SparkTrack.Core.Exceptions;
using SparkTrack.Core.Shared.Eventing;

namespace SparkTrack.Core.Services.Features;

using Archive;
using Authorization;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;
using Shared.Extensions;
using Shared.Services.Features;
using Shared.Services.SubTasks;
using Transactions;

internal class FeaturesService(
    IFeaturesRepository featuresRepository,
    ISubTasksRepository subTasksRepository,
    ISubTasksService subTasksService,
    IAuthorizationService authorizationService,
    IFeatureArchiveService featureArchiveService,
    IEventEmitter eventEmitter,
    ITransactionWrapper transactionWrapper
)
    : IFeaturesService
{
    public Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        bool showOnlyMine = true,
        FeatureFilterQuery? filterQuery = null,
        SortQuery? sortQuery = null,
        PageQuery? pageQuery = null
    )
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        Guid? employeeFilter = currentUser.GetEmployeeIdOrNull();

        Guid? authorId = null;

        if (currentUser.Role.IsAnyRole(ERole.Admin) && showOnlyMine) authorId = currentUser.Id;

        return featuresRepository.GetPageAsync(
            employeeFilter,
            authorId,
            filterQuery,
            sortQuery,
            pageQuery
        );
    }

    public Task<Feature?> GetAsync(int id)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        Guid? employeeFilter = currentUser.GetEmployeeIdOrNull();

        return featuresRepository.GetAsync(id, employeeFilter);
    }

    public async Task<int> AddAsync(FeatureEdit feature)
    {
        var addedFeature = await featuresRepository.AddAsync(feature);

        await eventEmitter.RaiseAsync(new FeatureCreatedEvent(addedFeature));

        return addedFeature.Id;
    }

    public async Task EditAsync(FeatureEdit feature)
    {
        var oldInfo = await featuresRepository.GetAsync(feature.Id, null);

        if (oldInfo is null) throw new NotFoundException();

        var newInfo = await featuresRepository.EditAsync(feature);

        await eventEmitter.RaiseAsync(new FeatureUpdatedEvent(oldInfo, newInfo));
    }

    public async Task DeleteAsync(int id, bool force)
    {
        if (force)
        {
            var feature = await featuresRepository.GetAsync(id, null);

            if (feature is null) throw new NotFoundException();

            await featuresRepository.DeleteAsync(id);
            await eventEmitter.RaiseAsync(new FeatureDeletedEvent(feature));
        }

        await featureArchiveService.ArchiveAsync(id, EArchiveSource.User);
    }

    public async Task SendOnPaymentAsync(IReadOnlyList<int> featuresIdList)
    {
        var tasksIdentities = await subTasksRepository.GetIdentityListByFeatureIdListAsync(featuresIdList);

        await transactionWrapper.ExecuteInTransactionAsync(async () =>
            {
                foreach (var identity in tasksIdentities)
                    await subTasksService.SetPaymentStatusAsync(
                        identity.Id,
                        EPaymentStatus.OnPayment,
                        identity.Version
                    );
            }
        );
    }
}