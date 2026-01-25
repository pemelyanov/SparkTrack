namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Data.Entities;
using Core.Exceptions;
using Core.Repositories;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Transactions;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

internal class SubTasksRepository(SparkTrackDbContext dbContext, ITransactionWrapper transactionWrapper) : ISubTasksRepository
{
    public Task<SubTask?> GetAsync(Guid id) => dbContext.SubTasks
        .AsNoTracking()
        .Where(it => it.Id == id)
        .Select(
            GetToSubTaskExpression()
        )
        .FirstOrDefaultAsync();

    public async Task<IReadOnlyList<SubTaskWithPayments>> GetListAsync(IReadOnlyList<Guid> idList) => await dbContext
        .SubTasks
        .Where(it => idList.Contains(it.Id))
        .Select(GetToSubTaskWithPaymentsExpression())
        .ToArrayAsync();

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
        var editedTasks = new List<SubTask>();

        await transactionWrapper.ExecuteInTransactionAsync(async () =>
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
            }
        );

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

    private Expression<Func<SubTaskData, SubTaskWithPayments>> GetToSubTaskWithPaymentsExpression() => data =>
        new SubTaskWithPayments
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
            IsTimelyBonusApproved = data.IsTimelyBonusApproved,
            Payments = data.Payments.Select(
                    p => new PaymentInfo
                    {
                        Id = p.Id,
                        Payment = p.Payment,
                        PaymentType = p.PaymentType,
                        TaskId = p.TaskId,
                        Admin = new User
                        {
                            Id = p.Admin.Id,
                            Email = p.Admin.Email,
                            Name = p.Admin.Name,
                            Role = p.Admin.Role,
                            TelegramTag = p.Admin.TelegramTag
                        }
                    }
                )
                .ToArray(),
            RemainingMainPayment =
                Math.Max(
                    data.Cost - data.Payments.Where(p => p.PaymentType == EPaymentType.Main).Sum(p => p.Payment),
                    0
                ),
            RemainingTimelyBonusPayment = Math.Max(
                data.TimelyBonus - data.Payments.Where(p => p.PaymentType == EPaymentType.TimelyBonus)
                    .Sum(p => p.Payment),
                0
            )
        };
}