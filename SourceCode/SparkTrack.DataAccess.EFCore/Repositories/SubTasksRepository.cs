namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Exceptions;
using Core.Repositories;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

public class SubTasksRepository(SparkTrackDbContext dbContext) : ISubTasksRepository
{
    public Task<User?> GetExecutorAsync(Guid id) => dbContext.SubTasks
        .AsNoTracking()
        .Where(it => it.Id == id)
        .Select(
            it => new User
            {
                Id = it.ExecutorEmployee.Id,
                Email = it.ExecutorEmployee.Email,
                Name = it.ExecutorEmployee.Name,
                Role = it.ExecutorEmployee.Role
            }
        )
        .FirstOrDefaultAsync();

    public async Task<SubTask?> SetIsCompletedAsync(Guid id, bool value, Guid currentVersion)
    {
        return await UpdateSubTaskAsync(id, UpdateAction);

        void UpdateAction(SubTaskData subTask)
        {
            subTask.IsCompleted = value;
            subTask.Version = currentVersion;
        }
    }

    public async Task<SubTask?> SetPaymentStatusAsync(Guid id, EPaymentStatus value, Guid currentVersion)
    {
        return await UpdateSubTaskAsync(id, UpdateAction);

        void UpdateAction(SubTaskData subTask)
        {
            subTask.PaymentStatus = value;
            subTask.Version = currentVersion;
        }
    }

    private async Task<SubTask?> UpdateSubTaskAsync(Guid id, Action<SubTaskData> updateAction)
    {
        var subTask = await dbContext.SubTasks.Where(it => it.Id == id)
            .Include(it => it.ExecutorEmployee)
            .FirstOrDefaultAsync();

        if (subTask is null) return null;

        updateAction(subTask);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConflictException("SubTask was modified early", e);
        }
        
        return ToSubTask(subTask);
    }

    private SubTask ToSubTask(SubTaskData data) => new()
    {
        Id = data.Id,
        Name = data.Name,
        ExecutorEmployee = new User
        {
            Id = data.ExecutorEmployee.Id,
            Email = data.ExecutorEmployee.Email,
            Name = data.ExecutorEmployee.Name,
            Role = data.ExecutorEmployee.Role
        },
        Deadline = data.Deadline,
        Cost = data.Cost,
        Version = data.Version,
        IsCompleted = data.IsCompleted,
        PaymentStatus = data.PaymentStatus
    };
}