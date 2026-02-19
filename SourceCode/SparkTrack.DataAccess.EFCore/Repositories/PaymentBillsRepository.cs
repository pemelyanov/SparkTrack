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
    public Task<PaymentInfo?> GetPaymentAsync(Guid id) => dbContext.Payments.AsNoTracking()
        .Where(it => it.Id == id)
        .Select(it => new PaymentInfo
            {
                Id = it.Id,
                Admin = new User
                {
                    Id = it.Admin.Id,
                    Email = it.Admin.Email,
                    Name = it.Admin.Name,
                    Role = it.Admin.Role,
                    TelegramTag = it.Admin.TelegramTag,
                    ArchivedAt = it.Admin.ArchivedAt,
                    ArchiveSource = it.Admin.ArchiveSource,
                },
                Payment = it.Payment,
                PaymentType = it.PaymentType,
                TaskId = it.TaskId,
                CreatedAt = it.CreatedAt
            }
        )
        .FirstOrDefaultAsync();

    public Task<BonusPaymentInfo?> GetBonusPaymentAsync(Guid id) => dbContext.Bonuses.AsNoTracking()
        .Where(it => it.Id == id)
        .Select(it => new BonusPaymentInfo
            {
                Id = it.Id,
                Admin = new User
                {
                    Id = it.Admin.Id,
                    Email = it.Admin.Email,
                    Name = it.Admin.Name,
                    Role = it.Admin.Role,
                    TelegramTag = it.Admin.TelegramTag,
                    ArchivedAt = it.Admin.ArchivedAt,
                    ArchiveSource = it.Admin.ArchiveSource,
                },
                Employee = new User
                {
                    Id = it.Employee.Id,
                    Email = it.Employee.Email,
                    Name = it.Employee.Name,
                    Role = it.Employee.Role,
                    TelegramTag = it.Employee.TelegramTag,
                    ArchivedAt = it.Employee.ArchivedAt,
                    ArchiveSource = it.Employee.ArchiveSource,
                },
                Payment = it.Payment,
                CreatedAt = it.CreatedAt,
                Comment = it.Comment,
            }
        )
        .FirstOrDefaultAsync();

    public async Task<IReadOnlyPagedData<PaymentBill>> GetPageAsync(
        bool isPaid,
        Guid? employeeId,
        Guid? projectId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    )
    {
        var targetPaymentStatus = isPaid ? EPaymentStatus.Paid : EPaymentStatus.OnPayment;

        var page = await dbContext.SubTasks
            .AsNoTracking()
            .WhereIf(projectId is not null, it => it.Feature.ProjectId == projectId)
            .WhereIf(employeeId is not null, it => it.ExecutorEmployeeId == employeeId)
            .WhereIf(startDate is not null, it => it.Feature.CreatedAt >= startDate)
            .WhereIf(endDate is not null, it => it.Feature.CreatedAt <= endDate)
            // TODO: Add filter
            .Where(it => it.Feature.ArchivedAt == null)
            .Where(it => it.ExecutorEmployee.ArchivedAt == null)
            .Where(it => it.PaymentStatus == targetPaymentStatus)
            .Select(data => new PaymentBill
                {
                    Feature = new Feature
                    {
                        Id = data.Feature.Id,
                        Name = data.Feature.Name,
                        Project = new Project
                        {
                            Id = data.Feature.Project.Id,
                            Name = data.Feature.Project.Name,
                            Link = data.Feature.Project.Link,
                            ArchivedAt = data.Feature.Project.ArchivedAt,
                            ArchiveSource = data.Feature.Project.ArchiveSource
                        },
                        CreatedAt = data.Feature.CreatedAt,
                        EditedAt = data.Feature.EditedAt,
                        ArchivedAt = data.Feature.ArchivedAt,
                        ArchiveSource = data.Feature.ArchiveSource
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
                            TelegramTag = data.ExecutorEmployee.TelegramTag,
                            ArchivedAt = data.ExecutorEmployee.ArchivedAt,
                            ArchiveSource = data.ExecutorEmployee.ArchiveSource
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
                    PaymentsList = data.Payments.Select(p => new PaymentInfo
                            {
                                Id = p.Id,
                                Admin = new User
                                {
                                    Id = p.Admin.Id,
                                    Email = p.Admin.Email,
                                    Name = p.Admin.Name,
                                    Role = p.Admin.Role,
                                    TelegramTag = p.Admin.TelegramTag,
                                    ArchivedAt = p.Admin.ArchivedAt,
                                    ArchiveSource = p.Admin.ArchiveSource
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
            // TODO: Add filter
            .Where(it => it.Feature.ArchivedAt == null)
            .Where(it => it.ExecutorEmployee.ArchivedAt == null)
            .GroupBy(it => it.ExecutorEmployee)
            .Select(grouping => new UserPayment
                {
                    User = new User
                    {
                        Id = grouping.Key.Id,
                        Email = grouping.Key.Email,
                        Name = grouping.Key.Name,
                        Role = grouping.Key.Role,
                        TelegramTag = grouping.Key.TelegramTag,
                        ArchivedAt = grouping.Key.ArchivedAt,
                        ArchiveSource = grouping.Key.ArchiveSource
                    },
                    Payment = grouping.Sum(it => it.IsTimelyBonusApproved
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

    public async Task<PendingPaymentsSummary> GetPendingPaymentsSummaryAsync(Guid? projectId)
    {
        var userRemainingPayments = await GetUsersRemainingPaymentsAsync(projectId);

        var adminPaidPayments = await dbContext.SubTasks
            .AsNoTracking()
            .WhereIf(projectId is not null, it => it.Feature.ProjectId == projectId)
            .Where(it => it.PaymentStatus == EPaymentStatus.OnPayment)
            // TODO: Add filter
            .Where(it => it.Feature.ArchivedAt == null)
            .Where(it => it.ExecutorEmployee.ArchivedAt == null)
            .SelectMany(it => it.Payments)
            .GroupBy(it => it.Admin)
            .Select(grouping => new UserPayment
                {
                    User = new User
                    {
                        Id = grouping.Key.Id,
                        Email = grouping.Key.Email,
                        Name = grouping.Key.Name,
                        Role = grouping.Key.Role,
                        TelegramTag = grouping.Key.TelegramTag,
                        ArchivedAt = grouping.Key.ArchivedAt,
                        ArchiveSource = grouping.Key.ArchiveSource
                    },
                    Payment = grouping.Sum(it => it.Payment
                    )
                }
            )
            .Where(it => it.Payment > 0)
            .ToArrayAsync();

        return new PendingPaymentsSummary
        {
            AdminPayments = adminPaidPayments,
            RemainingPayments = userRemainingPayments
        };
    }

    public async Task<IReadOnlyPagedData<PaymentDetails>> GetPaidPaymentsListAsync(
        Guid? adminId,
        Guid? employeeId,
        Guid? projectId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    ) => await dbContext
        .Payments
        .AsNoTracking()
        .WhereIf(adminId is not null, it => it.AdminId == adminId)
        .WhereIf(projectId is not null, it => it.Task.Feature.ProjectId == projectId)
        .WhereIf(employeeId is not null, it => it.Task.ExecutorEmployeeId == employeeId)
        .WhereIf(startDate is not null, it => it.CreatedAt >= startDate)
        .WhereIf(endDate is not null, it => it.CreatedAt <= endDate)
        .Select(data => new PaymentDetails
            {
                Id = data.Id,
                Admin = new User
                {
                    Id = data.Admin.Id,
                    Email = data.Admin.Email,
                    Name = data.Admin.Name,
                    Role = data.Admin.Role,
                    TelegramTag = data.Admin.TelegramTag,
                    ArchivedAt = data.Admin.ArchivedAt,
                    ArchiveSource = data.Admin.ArchiveSource
                },
                Payment = data.Payment,
                PaymentType = data.PaymentType,
                TaskId = data.TaskId,
                CreatedAt = data.CreatedAt,
                Task = new SubTask
                {
                    Id = data.Task.Id,
                    Name = data.Task.Name,
                    ExecutorEmployee = new User
                    {
                        Id = data.Task.ExecutorEmployee.Id,
                        Email = data.Task.ExecutorEmployee.Email,
                        Name = data.Task.ExecutorEmployee.Name,
                        Role = data.Task.ExecutorEmployee.Role,
                        TelegramTag = data.Task.ExecutorEmployee.TelegramTag,
                        ArchivedAt = data.Task.ExecutorEmployee.ArchivedAt,
                        ArchiveSource = data.Task.ExecutorEmployee.ArchiveSource
                    },
                    Deadline = data.Task.Deadline,
                    Cost = data.Task.Cost,
                    Version = data.Task.Version,
                    IsCompleted = data.Task.IsCompleted,
                    PaymentStatus = data.Task.PaymentStatus,
                    CompletedAt = data.Task.CompletedAt,
                    TimelyBonus = data.Task.TimelyBonus,
                    IsTimelyBonusApproved = data.Task.IsTimelyBonusApproved
                },
                Feature = new Feature
                {
                    Id = data.Task.Feature.Id,
                    Name = data.Task.Feature.Name,
                    Project = new Project
                    {
                        Id = data.Task.Feature.Project.Id,
                        Name = data.Task.Feature.Project.Name,
                        Link = data.Task.Feature.Project.Link,
                        ArchivedAt = data.Task.Feature.Project.ArchivedAt,
                        ArchiveSource = data.Task.Feature.Project.ArchiveSource
                    },
                    CreatedAt = data.Task.Feature.CreatedAt,
                    EditedAt = data.Task.Feature.EditedAt,
                    ArchivedAt = data.Task.Feature.ArchivedAt,
                    ArchiveSource = data.Task.Feature.ArchiveSource
                }
            }
        )
        .OrderByDescending(it => it.CreatedAt)
        .AsPaginated(pageQuery)
        .CollectAsync();

    public async Task<IReadOnlyPagedData<BonusPaymentInfo>> GetPaidBonusPaymentsListAsync(
        Guid? adminId,
        Guid? employeeId,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    ) => await dbContext
        .Bonuses
        .AsNoTracking()
        .WhereIf(adminId is not null, it => it.AdminId == adminId)
        .WhereIf(employeeId is not null, it => it.EmployeeId == employeeId)
        .WhereIf(startDate is not null, it => it.CreatedAt >= startDate)
        .WhereIf(endDate is not null, it => it.CreatedAt <= endDate)
        .Select(data => new BonusPaymentInfo
            {
                Id = data.Id,
                Admin = new User
                {
                    Id = data.Admin.Id,
                    Email = data.Admin.Email,
                    Name = data.Admin.Name,
                    Role = data.Admin.Role,
                    TelegramTag = data.Admin.TelegramTag,
                    ArchivedAt = data.Admin.ArchivedAt,
                    ArchiveSource = data.Admin.ArchiveSource
                },
                Employee = new User
                {
                    Id = data.Employee.Id,
                    Email = data.Employee.Email,
                    Name = data.Employee.Name,
                    Role = data.Employee.Role,
                    TelegramTag = data.Employee.TelegramTag,
                    ArchivedAt = data.Employee.ArchivedAt,
                    ArchiveSource = data.Employee.ArchiveSource
                },
                Payment = data.Payment,
                CreatedAt = data.CreatedAt,
                Comment = data.Comment,
            }
        )
        .OrderByDescending(it => it.CreatedAt)
        .AsPaginated(pageQuery)
        .CollectAsync();

    public async Task AddPaymentsRangeAsync(IReadOnlyList<PaymentInfo> paymentsList)
    {
        var paymentsDataList = paymentsList.Select(it => new PaymentData
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
            EmployeeId = bonusPaymentInfo.Employee.Id
        };

        await dbContext.Bonuses.AddAsync(bonusPaymentDate);

        await dbContext.SaveChangesAsync();
    }

    public async Task DeletePaymentAsync(Guid id)
    {
        var entity = await dbContext.Payments.FindAsync(id);

        if (entity is null) return;

        dbContext.Payments.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteBonusPaymentAsync(Guid id)
    {
        var entity = await dbContext.Bonuses.FindAsync(id);

        if (entity is null) return;

        dbContext.Bonuses.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsPaymentPaidByThisUser(Guid paymentId, Guid userId)
    {
        var payerId = await dbContext.Payments.AsNoTracking()
            .Where(it => it.Id == paymentId)
            .Select(it => it.AdminId)
            .FirstOrDefaultAsync();

        return payerId == userId;
    }

    public async Task<bool> IsBonusPaymentPaidByThisUser(Guid paymentId, Guid userId)
    {
        var payerId = await dbContext.Bonuses.AsNoTracking()
            .Where(it => it.Id == paymentId)
            .Select(it => it.AdminId)
            .FirstOrDefaultAsync();

        return payerId == userId;
    }
}