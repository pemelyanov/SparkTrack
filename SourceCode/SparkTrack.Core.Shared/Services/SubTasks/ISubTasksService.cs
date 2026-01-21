namespace SparkTrack.Core.Shared.Services.SubTasks;

using Data;
using Data.Entities;
using Enums;

public interface ISubTasksService
{
    Task<SubTask?> SetIsTimelyBonusApprovedAsync(Guid id, bool value, Guid currentVersion);
    
    Task<SubTask?> SetIsCompletedAsync(Guid id, bool value, Guid currentVersion);

    Task<SubTask?> SetPaymentStatusAsync(Guid id, EPaymentStatus value, Guid currentVersion);

    Task<IReadOnlyList<SubTask>> SetIsTimelyBonusApprovedAsync(
        IReadOnlyList<EditableEntityIdentity> identitiesList,
        bool value
    );
}