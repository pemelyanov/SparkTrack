using SparkTrack.Core.Events;
using SparkTrack.Core.Shared.Eventing;

namespace SparkTrack.Core.Services.SubTasks;

using Authorization;
using Exceptions;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Entities;
using Shared.Enums;
using Shared.Services.SubTasks;

public class SubTasksService(
    IAuthorizationService authorizationService,
    ISubTasksRepository subTasksRepository,
    IEventEmitter eventEmitter)
    : ISubTasksService
{
    public async Task<SubTask?> SetIsTimelyBonusApprovedAsync(Guid id, bool value, Guid currentVersion)
    {
        var subTask = await subTasksRepository.GetAsync(id);

        if (subTask is null) return null;

        subTask = subTask with
        {
            IsTimelyBonusApproved = value,
            Version = currentVersion
        };

        return await subTasksRepository.EditAsync(subTask);
    }

    public async Task<SubTask?> SetIsCompletedAsync(Guid id, bool value, Guid currentVersion)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        var subTask = await subTasksRepository.GetAsync(id);

        if (subTask is null) return null;

        var canEdit = currentUser.Role is ERole.Admin || currentUser.Id == subTask.ExecutorEmployee.Id;

        if (!canEdit) throw new ForbiddenException();

        var isCompleted = subTask.IsCompleted;
        var paymentStatus = EPaymentStatus.None;
        DateTime? completedAt = null;
        var isBonusApproved = subTask.IsTimelyBonusApproved;

        if (isCompleted)
        {
            isCompleted = false;
        }
        else
        {
            isCompleted = true;
            paymentStatus = EPaymentStatus.OnPayment;
            completedAt = DateTime.UtcNow;
            if (completedAt <= subTask.Deadline) isBonusApproved = true;
        }

        subTask = subTask with
        {
            IsCompleted = isCompleted,
            PaymentStatus = paymentStatus,
            Version = currentVersion,
            CompletedAt = completedAt,
            IsTimelyBonusApproved = isBonusApproved
        };

        var updatedSubTask = await subTasksRepository.EditAsync(subTask);

        if (updatedSubTask is null) return null;

        if (updatedSubTask.IsCompleted)
            await eventEmitter.RaiseAsync(new SubTaskCompletedEvent(updatedSubTask));

        return updatedSubTask;
    }

    public async Task<SubTask?> SetPaymentStatusAsync(Guid id, EPaymentStatus value, Guid currentVersion)
    {
        var subTask = await subTasksRepository.GetAsync(id);

        if (subTask is null) return null;

        subTask = subTask with
        {
            PaymentStatus = value,
            Version = currentVersion
        };

        return await subTasksRepository.EditAsync(subTask);
    }

    public async Task<IReadOnlyList<SubTask>> SetIsTimelyBonusApprovedAsync(
        IReadOnlyList<EditableEntityIdentity> identitiesList,
        bool value
    )
    {
        var subTasksToUpdate = new List<SubTask>(identitiesList.Count);

        foreach (var identity in identitiesList)
        {
            var subTask = await subTasksRepository.GetAsync(identity.Id);

            if (subTask is null) continue;

            subTasksToUpdate.Add(
                subTask with
                {
                    IsTimelyBonusApproved = value,
                    Version = identity.Version
                }
            );
        }

        return await subTasksRepository.EditRangeAsync(subTasksToUpdate);
    }
}