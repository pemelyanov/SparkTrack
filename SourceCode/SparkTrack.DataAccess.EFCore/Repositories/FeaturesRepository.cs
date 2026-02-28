using LinqKit;

namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Exceptions;
using System.Linq.Expressions;
using Core.Repositories;
using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Transactions;
using Data.Entities;
using Extensions;
using Microsoft.EntityFrameworkCore;

internal sealed class FeaturesRepository(SparkTrackDbContext dbContext, ITransactionWrapper transactionWrapper)
    : IFeaturesRepository
{
    public Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        Guid? projectId,
        bool showCompleted,
        Guid? subTaskEmployeeId,
        DateTime? startDate,
        DateTime? endDate,
        Guid? authorId,
        SortQuery? sortQuery,
        PageQuery pageQuery
    ) => dbContext.Features
        .AsNoTracking()
        .WhereIf(projectId is not null, f => f.ProjectId == projectId)
        .WhereIf(startDate is not null, it => it.CreatedAt >= startDate)
        .WhereIf(endDate is not null, it => it.CreatedAt <= endDate)
        .WhereIf(
            subTaskEmployeeId is not null,
            f => f.TasksList.Any(t => t.ExecutorEmployeeId == subTaskEmployeeId)
        )
        .WhereIf(
            !showCompleted,
            f => f.TasksList.Count == 0
                || f.TasksList.Any(t => !t.IsCompleted || t.PaymentStatus != EPaymentStatus.Paid)
        )
        .WhereIf(authorId is not null, it => it.AuthorsList.Count == 0 || it.AuthorsList.Any(a => a.Id == authorId))
        .OrderBy(
            sortQuery,
            () => sortQuery?.SortField switch
            {
                "Name" => it => it.Name,
                "Deadline" => it => it.TasksList.Select(t => t.Deadline).Min(),
                "CreatedAt" => it => it.CreatedAt,
                _ => throw new NotSupportedException(sortQuery?.SortField)
            }
        )
        // TODO: Add filter
        .Where(it => it.ArchivedAt == null)
        .AsExpandableEFCore()
        .Select(GetFeatureMapExpression(subTaskEmployeeId))
        .AsPaginated(pageQuery)
        .CollectAsync();

    public Task<Feature?> GetAsync(
        int id,
        Guid? subTaskEmployeeId
    ) => dbContext.Features
        .AsNoTracking()
        .Where(f => f.Id == id)
        .AsExpandableEFCore()
        .Select(GetFeatureMapExpression(subTaskEmployeeId))
        .FirstOrDefaultAsync();

    public async Task<Feature> AddAsync(FeatureEdit feature)
    {
        var subTasksDataMap = feature.TasksList.Select(
                ToSubTaskData
            )
            .ToDictionary(it => it.Id);

        var featureData = new FeatureData
        {
            Name = feature.Name,
            ProjectId = feature.ProjectId,
            Description = feature.Description,
            TasksList = subTasksDataMap.Values,
            AttachmentsList = feature.AttachmentsList.Select(
                    AttachmentsUtils.ToAttachmentData
                )
                .ToArray(),
            CreatedAt = DateTime.UtcNow
        };

        return await transactionWrapper.ExecuteInTransactionAsync(async () =>
            {
                var authors = await dbContext.Users.Where(it => feature.AuthorsIdList.Contains(it.Id))
                    .ToArrayAsync();

                foreach (var userData in authors)
                    featureData.AuthorsList.Add(userData);

                var addedFeature = await dbContext.Features.AddAsync(featureData);
                await dbContext.SaveChangesAsync();

                foreach (var subTaskEdit in feature.TasksList)
                {
                    var subTask = subTasksDataMap[subTaskEdit.Id];

                    foreach (var dependencyId in subTaskEdit.DependsOnIdList)
                    {
                        var dependency = subTasksDataMap[dependencyId];

                        subTask.DependsOnList.Add(dependency);
                    }
                }

                await dbContext.SaveChangesAsync();

                return await dbContext.Features.Where(it => it.Id == addedFeature.Entity.Id)
                    .AsExpandableEFCore()
                    .Select(GetFeatureMapExpression())
                    .FirstAsync();
            }
        );
    }

    private static SubTaskData ToSubTaskData(SubTaskEdit t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        ExecutorEmployeeId = t.ExecutorEmployeeId,
        Deadline = t.Deadline,
        Cost = t.Cost,
        IsCompleted = t.IsCompleted,
        PaymentStatus = t.PaymentStatus,
        Version = t.Version,
        TimelyBonus = t.TimelyBonus,
    };

    public async Task<Feature> EditAsync(FeatureEdit feature)
    {
        var featureData = await dbContext.Features
            .Include(f => f.TasksList)
            .ThenInclude(t => t.DependsOnList)
            .Include(f => f.AttachmentsList)
            .Include(f => f.AuthorsList)
            .FirstOrDefaultAsync(f => f.Id == feature.Id);

        if (featureData is null)
        {
            throw new NotFoundException($"Feature with id {feature.Id} not found");
        }

        return await transactionWrapper.ExecuteInTransactionAsync(async () =>
            {
                featureData.Name = feature.Name;
                featureData.Description = feature.Description;
                featureData.Version = feature.Version;
                featureData.EditedAt = DateTime.UtcNow;

                var tasksMap = HandleSubTasks(feature, featureData);

                AttachmentsUtils.HandleAttachments(dbContext, feature.AttachmentsList, featureData);

                await HandleAuthorsAsync(feature, featureData);

                try
                {
                    await dbContext.SaveChangesAsync();

                    foreach (var subTaskEdit in feature.TasksList)
                    {
                        var subTask = tasksMap[subTaskEdit.Id];

                        foreach (var dependencyId in subTaskEdit.DependsOnIdList)
                        {
                            var dependency = tasksMap[dependencyId];

                            subTask.DependsOnList.Add(dependency);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    throw new ConflictException("Feature was modified early", e);
                }

                return await dbContext.Features.Where(it => it.Id == feature.Id)
                    .AsExpandableEFCore()
                    .Select(GetFeatureMapExpression())
                    .FirstAsync();
            }
        );
    }

    private async Task HandleAuthorsAsync(FeatureEdit feature, FeatureData featureData)
    {
        var existingAuthors = featureData.AuthorsList
            .ToDictionary(t => t.Id);

        foreach (var authorId in feature.AuthorsIdList)
        {
            if (existingAuthors.Remove(authorId))
            {
                continue;
            }

            var author = await dbContext.Users.FindAsync(authorId);

            if (author is null) continue;

            featureData.AuthorsList.Add(author);

            existingAuthors.Remove(authorId);
        }

        foreach (var author in existingAuthors.Values)
            featureData.AuthorsList.Remove(author);
    }

    private IDictionary<Guid, SubTaskData> HandleSubTasks(FeatureEdit feature, FeatureData featureData)
    {
        var existingTasks = featureData.TasksList
            .ToDictionary(t => t.Id);

        Dictionary<Guid, SubTaskData> editDataMap = [];

        foreach (var taskEdit in feature.TasksList)
        {
            if (!existingTasks.ContainsKey(taskEdit.Id))
            {
                var data = ToSubTaskData(taskEdit);
                featureData.TasksList.Add(
                    data
                );

                editDataMap[taskEdit.Id] = data;

                continue;
            }

            if (!existingTasks.TryGetValue(taskEdit.Id, out var existingTask))
            {
                continue;
            }

            existingTask.Name = taskEdit.Name;
            existingTask.ExecutorEmployeeId = taskEdit.ExecutorEmployeeId;
            existingTask.Cost = taskEdit.Cost;
            existingTask.TimelyBonus = taskEdit.TimelyBonus;
            existingTask.Version = taskEdit.Version;
            existingTask.Deadline = taskEdit.Deadline;

            editDataMap[taskEdit.Id] = existingTask;

            existingTasks.Remove(taskEdit.Id);
        }

        if (existingTasks.Count > 0)
        {
            dbContext.SubTasks.RemoveRange(existingTasks.Values);
        }

        return editDataMap;
    }

    public async Task DeleteAsync(int id)
    {
        var feature = await dbContext.Features
            .Include(f => f.TasksList)
            .Include(featureData => featureData.AttachmentsList)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (feature is null)
        {
            throw new InvalidOperationException($"Feature with id {id} not found");
        }

        dbContext.SubTasks.RemoveRange(feature.TasksList);
        dbContext.Attachments.RemoveRange(feature.AttachmentsList);
        dbContext.Features.Remove(feature);

        await dbContext.SaveChangesAsync();
    }

    public async Task SetArchiveStatus(int id, bool isArchived, EArchiveSource? archiveSource = null)
    {
        var feature = await dbContext.Features.FindAsync(id);

        if (feature is null) return;

        feature.ArchiveSource =
            isArchived ? archiveSource ?? throw new InvalidOperationException("Enter archive source") : null;

        feature.ArchivedAt = isArchived ? DateTime.UtcNow : null;

        await dbContext.SaveChangesAsync();
    }

    public static Expression<Func<FeatureData, Feature>>
        GetFeatureMapExpression(
            Guid? subTaskEmployeeId = null
        ) => f => new Feature
    {
        Id = f.Id,
        Name = f.Name,
        Description = f.Description,
        Project = GetProjectMapExpression().Invoke(f.Project),
        TasksList = f.TasksList
            .Where(t => subTaskEmployeeId == null || t.ExecutorEmployeeId == subTaskEmployeeId
                || t.DependentForList.Any(d => d.ExecutorEmployeeId == subTaskEmployeeId)
            )
            .OrderBy(t => t.Deadline)
            .Select(t => GetSubTaskMapExpression(subTaskEmployeeId).Invoke(t))
            .ToArray(),
        AttachmentsList = f.AttachmentsList
            .Select(a => GetAttachmentMapExpression().Invoke(a))
            .ToArray(),
        AuthorsList = f.AuthorsList.Select(a => GetUserMapExpression().Invoke(a)).ToArray(),
        CreatedAt = f.CreatedAt,
        EditedAt = f.EditedAt,
        Version = f.Version,
        ArchivedAt = f.ArchivedAt,
        ArchiveSource = f.ArchiveSource
    };

    private static Expression<Func<ProjectData, Project>> GetProjectMapExpression()
    {
        return project => new Project
        {
            Id = project.Id,
            Name = project.Name,
            Link = project.Link,
            ArchivedAt = project.ArchivedAt,
            ArchiveSource = project.ArchiveSource
        };
    }

    private static Expression<Func<UserData, User>> GetUserMapExpression()
    {
        return user => new User
        {
            Id = user.Id,
            Name = user.Name,
            Role = user.Role,
            TelegramTag = user.TelegramTag,
            Email = user.Email,
            ArchivedAt = user.ArchivedAt,
            ArchiveSource = user.ArchiveSource
        };
    }

    private static Expression<Func<SubTaskData, SubTask>> GetSubTaskMapExpression(Guid? employeeDataFilter = null)
    {
        return subTask => new SubTask
        {
            Id = subTask.Id,
            Name = subTask.Name,
            Deadline = subTask.Deadline,
            Cost = employeeDataFilter == null || employeeDataFilter == subTask.ExecutorEmployeeId ? subTask.Cost : 0,
            ExecutorEmployee = GetUserMapExpression().Invoke(subTask.ExecutorEmployee),
            PaymentStatus = subTask.PaymentStatus,
            IsCompleted = subTask.IsCompleted,
            Version = subTask.Version,
            CompletedAt = subTask.CompletedAt,
            TimelyBonus = employeeDataFilter == null || employeeDataFilter == subTask.ExecutorEmployeeId ? subTask.TimelyBonus : 0,
            IsTimelyBonusApproved = subTask.IsTimelyBonusApproved,
            DependsOnIdList = subTask.DependsOnList.Select(s => s.Id).ToArray()
        };
    }

    private static Expression<Func<AttachmentData, Attachment>> GetAttachmentMapExpression()
    {
        return attachment => new Attachment
        {
            Id = attachment.Id,
            Name = attachment.Name,
            Extension = attachment.Extension,
            Size = attachment.Size,
            FileId = attachment.FileId,
            IsImage = attachment.IsImage,
            Checksum = attachment.Checksum
        };
    }
}