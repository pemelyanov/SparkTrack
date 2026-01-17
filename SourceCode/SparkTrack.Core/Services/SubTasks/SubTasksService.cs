namespace SparkTrack.Core.Services.SubTasks;

using Authorization;
using Exceptions;
using Extensions;
using Repositories;
using Shared.Data.Entities;
using Shared.Enums;
using Shared.Services.SubTasks;

public class SubTasksService(IAuthorizationService authorizationService, ISubTasksRepository subTasksRepository)
    : ISubTasksService
{
    public async Task<SubTask?> SetIsCompletedAsync(Guid id, bool value, Guid currentVersion)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        var subTask = await subTasksRepository.GetAsync(id);

        if (subTask is null) return null;

        var canEdit = currentUser.Role is ERole.Admin || currentUser.Id == subTask.ExecutorEmployee.Id;

        if (!canEdit) throw new ForbiddenException();

        var isCompleted = subTask.IsCompleted;
        EPaymentStatus paymentStatus;
        
        if (isCompleted)
        {
            isCompleted = false;
            paymentStatus = EPaymentStatus.None;
        }
        else
        {
            isCompleted = true;
            paymentStatus = EPaymentStatus.OnPayment;   
        }

        subTask = subTask with
        {
            IsCompleted = isCompleted,
            PaymentStatus = paymentStatus,
            Version = currentVersion
        };

        return await subTasksRepository.EditAsync(subTask);
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
}