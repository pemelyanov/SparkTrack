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

        var executor = await subTasksRepository.GetExecutorAsync(id);

        if (executor is null) return null;

        var canEdit = currentUser.Role is ERole.Admin || currentUser.Id == executor.Id;

        if (!canEdit) throw new ForbiddenException();

        return await subTasksRepository.SetIsCompletedAsync(id, value, currentVersion);
    }

    public Task<SubTask?> SetPaymentStatusAsync(Guid id, EPaymentStatus value, Guid currentVersion)
    {
        return subTasksRepository.SetPaymentStatusAsync(id, value, currentVersion);
    }
}