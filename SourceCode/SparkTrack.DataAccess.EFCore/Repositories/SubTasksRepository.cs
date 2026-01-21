namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Exceptions;
using Core.Repositories;
using Core.Shared.Data.Entities;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class SubTasksRepository(SparkTrackDbContext dbContext) : ISubTasksRepository
{
    public Task<SubTask?> GetAsync(Guid id) => dbContext.SubTasks
        .AsNoTracking()
        .Where(it => it.Id == id)
        .Select(
            GetToSubTaskExpression()
        )
        .FirstOrDefaultAsync();

    public Task<SubTask?> EditAsync(SubTask subTask)
    {
        return UpdateSubTaskAsync(subTask, UpdateAction);

        void UpdateAction(SubTaskData subTaskData)
        {
            subTaskData.Cost = subTask.Cost;
            subTaskData.Deadline = subTask.Deadline;
            subTaskData.Name = subTask.Name;
            subTaskData.IsCompleted = subTask.IsCompleted;
            subTaskData.ExecutorEmployeeId = subTask.ExecutorEmployee.Id;
            subTaskData.PaymentStatus = subTask.PaymentStatus;
            subTaskData.Version = subTask.Version;
            subTaskData.TimelyBonus = subTask.TimelyBonus;
            subTaskData.IsTimelyBonusApproved = subTask.IsTimelyBonusApproved;
            subTaskData.CompletedAt = subTask.CompletedAt;
        }
    }

    public async Task<IReadOnlyList<SubTask>> EditRangeAsync(IReadOnlyList<SubTask> subTasksList)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync();
        var editedTasks = new List<SubTask>();

        try
        {
            foreach (var subTask in subTasksList)
            {
                try
                {
                    if (await EditAsync(subTask) is not { } edited) continue;

                    editedTasks.Add(edited);
                }
                catch (ConflictException)
                {
                    // ignore
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            return [];
        }

        return editedTasks;
    }

    private async Task<SubTask?> UpdateSubTaskAsync(SubTask subTask, Action<SubTaskData> updateAction)
    {
        var subTaskData = await dbContext.SubTasks.Where(it => it.Id == subTask.Id)
            .Include(it => it.ExecutorEmployee)
            .FirstOrDefaultAsync();

        if (subTaskData is null) return null;

        updateAction(subTaskData);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConflictException("SubTask was modified early", e);
        }

        subTaskData = await dbContext.SubTasks.Where(it => it.Id == subTask.Id)
            .Include(it => it.ExecutorEmployee)
            .FirstOrDefaultAsync();

        return GetToSubTaskExpression().Compile().Invoke(subTaskData!);
    }

    private Expression<Func<SubTaskData, SubTask>> GetToSubTaskExpression() => data => new SubTask
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
        PaymentStatus = data.PaymentStatus,
        CompletedAt = data.CompletedAt,
        TimelyBonus = data.TimelyBonus,
        IsTimelyBonusApproved = data.IsTimelyBonusApproved
    };
}