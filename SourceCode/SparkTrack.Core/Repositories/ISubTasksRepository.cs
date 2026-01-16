namespace SparkTrack.Core.Repositories;

using Shared.Data.Entities;
using Shared.Enums;

public interface ISubTasksRepository
{
    Task<User?> GetExecutorAsync(Guid id);
    
    Task<SubTask?> SetIsCompletedAsync(Guid id, bool value, Guid currentVersion);

    Task<SubTask?> SetPaymentStatusAsync(Guid id, EPaymentStatus value, Guid currentVersion);
}