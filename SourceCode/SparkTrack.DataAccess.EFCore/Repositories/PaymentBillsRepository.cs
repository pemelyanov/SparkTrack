namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Repositories;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Extensions;
using Microsoft.EntityFrameworkCore;

public class PaymentBillsRepository(SparkTrackDbContext dbContext) : IPaymentBillsRepository
{
    public async Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(
        bool isPaid,
        Guid? employeeId,
        Guid? projectId,
        PageQuery pageQuery
    )
    {
        var targetPaymentStatus = isPaid ? EPaymentStatus.Paid : EPaymentStatus.OnPayment;
        
        var page = await dbContext.SubTasks
            .AsNoTracking()
            .WhereIf(projectId is not null, it => it.Feature.ProjectId == projectId)
            .WhereIf(employeeId is not null, it => it.ExecutorEmployeeId == employeeId)
            .Where(it => it.PaymentStatus == targetPaymentStatus)
            .Select(
                data => new PaymentBill
                {
                    Feature = new Feature
                    {
                        Id = data.Feature.Id,
                        Name = data.Feature.Name,
                        Project = new Project
                        {
                            Id = data.Feature.Project.Id,
                            Name = data.Feature.Project.Name,
                            Link = data.Feature.Project.Link
                        },
                        CreatedAt = data.Feature.CreatedAt,
                        EditedAt = data.Feature.EditedAt
                    },
                    SubTask = new SubTask
                    {
                        Id = data.Id,
                        Name = data.Name,
                        ExecutorEmployee = new User
                        {
                            Id = data.ExecutorEmployee.Id,
                            Email = data.ExecutorEmployee.Email,
                            Name = data.ExecutorEmployee.Name,
                            Role = data.ExecutorEmployee.Role,
                            TelegramTag = data.ExecutorEmployee.TelegramTag
                        },
                        Deadline = data.Deadline,
                        Cost = data.Cost,
                        Version = data.Version,
                        IsCompleted = data.IsCompleted,
                        PaymentStatus = data.PaymentStatus,
                        CompletedAt = data.CompletedAt,
                        TimelyBonus = data.TimelyBonus,
                        IsTimelyBonusApproved = data.IsTimelyBonusApproved
                    }
                }
            )
            .AsPaginated(pageQuery)
            .CollectAsync();

        return page;
    }

    public async Task<IReadOnlyList<UserRemainingPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId)
    {
        return await dbContext.SubTasks
            .WhereIf(projectId is not null, it => it.Feature.ProjectId == projectId)
            .Where(it => it.PaymentStatus == EPaymentStatus.OnPayment)
            .Select(
                data => new UserRemainingPayment
                {
                    User = new User
                    {
                        Id = data.ExecutorEmployee.Id,
                        Email = data.ExecutorEmployee.Email,
                        Name = data.ExecutorEmployee.Name,
                        Role = data.ExecutorEmployee.Role,
                        TelegramTag = data.ExecutorEmployee.TelegramTag
                    },
                    Project = new Project
                    {
                        Id = data.Feature.Project.Id,
                        Name = data.Feature.Project.Name,
                        Link = data.Feature.Project.Link
                    },
                    RemainingPayment = data.IsTimelyBonusApproved ? data.Cost + data.TimelyBonus : data.Cost
                }
            )
            .ToArrayAsync();
    }
}