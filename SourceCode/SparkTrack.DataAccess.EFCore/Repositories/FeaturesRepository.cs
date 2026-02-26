using LinqKit;

namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Exceptions;
using System.Linq.Expressions;
using Core.Repositories;
using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Data.Entities;
using Extensions;
using Microsoft.EntityFrameworkCore;

internal sealed class FeaturesRepository(SparkTrackDbContext dbContext) : IFeaturesRepository
{
    public Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        Guid? projectId,
        bool showCompleted,
        Guid? subTaskEmployeeId,
        DateTime? startDate,
        DateTime? endDate,
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
        // TODO: Add filter
        .Where(it => it.ArchivedAt == null)
        .OrderByDescending(it => it.CreatedAt)
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
        var featureData = new FeatureData
        {
            Name = feature.Name,
            ProjectId = feature.ProjectId,
            Description = feature.Description,
            TasksList = feature.TasksList
                .Select(
                    ToSubTaskData
                )
                .ToList(),
            AttachmentsList = feature.AttachmentsList.Select(
                    AttachmentsUtils.ToAttachmentData
                )
                .ToArray(),
            CreatedAt = DateTime.UtcNow
        };

        var addedFeature = await dbContext.Features.AddAsync(featureData);
        await dbContext.SaveChangesAsync();

        return await dbContext.Features.Where(it => it.Id == addedFeature.Entity.Id)
            .Include(it => it.TasksList)
            .ThenInclude(it => it.ExecutorEmployee)
            .Include(it => it.Project)
            .AsExpandableEFCore()
            .Select(GetFeatureMapExpression(null))
            .FirstAsync();
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
            .Include(f => f.AttachmentsList)
            .FirstOrDefaultAsync(f => f.Id == feature.Id);

        if (featureData is null)
        {
            throw new InvalidOperationException($"Feature with id {feature.Id} not found");
        }

        featureData.Name = feature.Name;
        featureData.Description = feature.Description;
        featureData.Version = feature.Version;
        featureData.EditedAt = DateTime.UtcNow;

        HandleSubTasks(feature, featureData);

        AttachmentsUtils.HandleAttachments(dbContext, feature.AttachmentsList, featureData);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConflictException("Feature was modified early", e);
        }
        
        return await dbContext.Features.Where(it => it.Id == feature.Id)
            .Include(it => it.TasksList)
            .ThenInclude(it => it.ExecutorEmployee)
            .Include(it => it.Project)
            .AsExpandableEFCore()
            .Select(GetFeatureMapExpression(null))
            .FirstAsync();
    }

    private void HandleSubTasks(FeatureEdit feature, FeatureData featureData)
    {
        var existingTasks = featureData.TasksList
            .ToDictionary(t => t.Id);

        foreach (var taskEdit in feature.TasksList)
        {
            if (taskEdit.Id == Guid.Empty)
            {
                featureData.TasksList.Add(
                    ToSubTaskData(taskEdit)
                );

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

            existingTasks.Remove(taskEdit.Id);
        }

        if (existingTasks.Count > 0)
        {
            dbContext.SubTasks.RemoveRange(existingTasks.Values);
        }
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
        Guid? subTaskEmployeeId
    ) => f => new Feature
    {
        Id = f.Id,
        Name = f.Name,
        Description = f.Description,
        Project = GetProjectMapExpression().Invoke(f.Project),
        TasksList = f.TasksList
            .Where(t => subTaskEmployeeId == null || t.ExecutorEmployeeId == subTaskEmployeeId)
            .OrderBy(t => t.Deadline)
            .Select(t => GetSubTaskMapExpression().Invoke(t))
            .ToArray(),
        AttachmentsList = f.AttachmentsList
            .Select(a => GetAttachmentMapExpression().Invoke(a))
            .ToArray(),
        CreatedAt = f.CreatedAt,
        EditedAt = f.EditedAt,
        Version = f.Version,
        ArchivedAt = f.ArchivedAt,
        ArchiveSource = f.ArchiveSource
    };

    private static Expression<Func<ProjectData, Project>> GetProjectMapExpression()
    {
        return  project => new Project
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
    
    private static Expression<Func<SubTaskData, SubTask>> GetSubTaskMapExpression()
    {
        return subTask => new SubTask
        {
            Id = subTask.Id,
            Name = subTask.Name,
            Deadline = subTask.Deadline,
            Cost = subTask.Cost,
            ExecutorEmployee = GetUserMapExpression().Invoke(subTask.ExecutorEmployee),
            PaymentStatus = subTask.PaymentStatus,
            IsCompleted = subTask.IsCompleted,
            Version = subTask.Version,
            CompletedAt = subTask.CompletedAt,
            TimelyBonus = subTask.TimelyBonus,
            IsTimelyBonusApproved = subTask.IsTimelyBonusApproved
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