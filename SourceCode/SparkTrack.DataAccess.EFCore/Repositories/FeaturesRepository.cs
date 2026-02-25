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
        .OrderBy(it => it.CreatedAt)
        .Select(GetFeatureMapExpression(subTaskEmployeeId))
        .AsPaginated(pageQuery)
        .CollectAsync();

    public Task<Feature?> GetAsync(
        int id,
        Guid? subTaskEmployeeId
    ) => dbContext.Features
        .AsNoTracking()
        .Where(f => f.Id == id)
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

        featureData = await dbContext.Features.Where(it => it.Id == addedFeature.Entity.Id)
            .Include(it => it.TasksList)
            .ThenInclude(it => it.ExecutorEmployee)
            .Include(it => it.Project)
            .FirstAsync();

        return GetFeatureMapExpression(null).Compile().Invoke(featureData);
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
        
        featureData = await dbContext.Features.Where(it => it.Id == feature.Id)
            .Include(it => it.TasksList)
            .ThenInclude(it => it.ExecutorEmployee)
            .Include(it => it.Project)
            .FirstAsync();

        return GetFeatureMapExpression(null).Compile().Invoke(featureData);
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
        Project = new Project
        {
            Id = f.Project.Id,
            Name = f.Project.Name,
            Link = f.Project.Link,
            ArchivedAt = f.Project.ArchivedAt,
            ArchiveSource = f.Project.ArchiveSource
        },
        TasksList = f.TasksList
            .Where(t => subTaskEmployeeId == null || t.ExecutorEmployeeId == subTaskEmployeeId)
            .OrderBy(t => t.Deadline)
            .Select(t => new SubTask
                {
                    Id = t.Id,
                    Name = t.Name,
                    Deadline = t.Deadline,
                    Cost = t.Cost,
                    ExecutorEmployee = new User
                    {
                        Id = t.ExecutorEmployee.Id,
                        Name = t.ExecutorEmployee.Name,
                        Role = t.ExecutorEmployee.Role,
                        TelegramTag = t.ExecutorEmployee.TelegramTag,
                        Email = t.ExecutorEmployee.Email,
                        ArchivedAt = t.ExecutorEmployee.ArchivedAt,
                        ArchiveSource = t.ExecutorEmployee.ArchiveSource
                    },
                    PaymentStatus = t.PaymentStatus,
                    IsCompleted = t.IsCompleted,
                    Version = t.Version,
                    CompletedAt = t.CompletedAt,
                    TimelyBonus = t.TimelyBonus,
                    IsTimelyBonusApproved = t.IsTimelyBonusApproved
                }
            )
            .ToArray(),
        AttachmentsList = f.AttachmentsList
            .Select(a => new Attachment
                {
                    Id = a.Id,
                    Name = a.Name,
                    Extension = a.Extension,
                    Size = a.Size,
                    FileId = a.FileId,
                    IsImage = a.IsImage,
                    Checksum = a.Checksum
                }
            )
            .ToArray(),
        CreatedAt = f.CreatedAt,
        EditedAt = f.EditedAt,
        Version = f.Version,
        ArchivedAt = f.ArchivedAt,
        ArchiveSource = f.ArchiveSource
    };
}