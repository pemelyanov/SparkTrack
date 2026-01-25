namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Repositories;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Data.Entities;
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
                    },
                    PaymentsList = data.Payments.Select(
                            p => new PaymentInfo
                            {
                                Id = p.Id,
                                Admin = new User
                                {
                                    Id = p.Admin.Id,
                                    Email = p.Admin.Email,
                                    Name = p.Admin.Name,
                                    Role = p.Admin.Role,
                                    TelegramTag = p.Admin.TelegramTag
                                },
                                Payment = p.Payment,
                                PaymentType = p.PaymentType,
                                TaskId = p.TaskId,
                                CreatedAt = p.CreatedAt
                            }
                        )
                        .ToArray()
                }
            )
            .AsPaginated(pageQuery)
            .CollectAsync();

        return page;
    }

    public async Task<IReadOnlyList<UserPayment>> GetUsersRemainingPaymentsAsync(Guid? projectId)
    {
        return await dbContext.SubTasks
            .WhereIf(projectId is not null, it => it.Feature.ProjectId == projectId)
            .Where(it => it.PaymentStatus == EPaymentStatus.OnPayment)
            .GroupBy(it => it.ExecutorEmployee)
            .Select(
                grouping => new UserPayment
                {
                    User = new User
                    {
                        Id = grouping.Key.Id,
                        Email = grouping.Key.Email,
                        Name = grouping.Key.Name,
                        Role = grouping.Key.Role,
                        TelegramTag = grouping.Key.TelegramTag
                    },
                    Payment = grouping.Sum(
                        it => it.IsTimelyBonusApproved
                            ? Math.Max(
                                it.Cost - it.Payments.Where(p => p.PaymentType == EPaymentType.Main)
                                    .Sum(p => p.Payment),
                                0
                            ) + Math.Max(
                                it.TimelyBonus - it.Payments.Where(p => p.PaymentType == EPaymentType.TimelyBonus)
                                    .Sum(p => p.Payment),
                                0
                            )
                            : Math.Max(
                                it.Cost - it.Payments.Where(p => p.PaymentType == EPaymentType.Main)
                                    .Sum(p => p.Payment),
                                0
                            )
                    )
                }
            )
            .Where(it => it.Payment > 0)
            .ToArrayAsync();
    }

    public async Task AddPaymentsRangeAsync(IReadOnlyList<PaymentInfo> paymentsList)
    {
        var paymentsDataList = paymentsList.Select(
                it => new PaymentData
                {
                    Id = it.Id,
                    AdminId = it.Admin.Id,
                    Payment = it.Payment,
                    CreatedAt = it.CreatedAt,
                    PaymentType = it.PaymentType,
                    TaskId = it.TaskId
                }
            )
            .ToArray();

        await dbContext.Payments.AddRangeAsync(paymentsDataList);

        await dbContext.SaveChangesAsync();
    }

    public async Task AddBonusPaymentAsync(BonusPaymentInfo bonusPaymentInfo)
    {
        var bonusPaymentDate = new BonusPaymentData
        {
            Id = bonusPaymentInfo.Id,
            AdminId = bonusPaymentInfo.Admin.Id,
            Comment = bonusPaymentInfo.Comment,
            Payment = bonusPaymentInfo.Payment,
            CreatedAt = bonusPaymentInfo.CreatedAt,
            EmployeeId = bonusPaymentInfo.EmployeeId
        };

        await dbContext.Bonuses.AddAsync(bonusPaymentDate);

        await dbContext.SaveChangesAsync();
    }

    public Task DeletePaymentAsync(Guid id) => throw new NotImplementedException();

    public Task DeleteBonusPaymentAsync(Guid id) => throw new NotImplementedException();
}