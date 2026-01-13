namespace SparkTrack.DataAccess.EFCore.Repositories;

using System.Linq.Expressions;
using Core.Repositories;
using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Data.Entities;
using Extensions;
using Microsoft.EntityFrameworkCore;

internal sealed class FeaturesRepository(SparkTrackDbContext dbContext) : IFeaturesRepository
{
    public Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        Guid? projectId,
        bool showCompleted,
        Guid? subTaskEmployeeId,
        PageQuery pageQuery
    ) => dbContext.Features
        .AsNoTracking()
        .WhereIf(projectId is not null, f => f.ProjectId == projectId)
        .WhereIf(
            subTaskEmployeeId is not null,
            f => f.TasksList.Any(t => t.ExecutorEmployeeId == subTaskEmployeeId)
        )
        .WhereIf(
            !showCompleted,
            f => f.TasksList.Any(t => !t.IsCompleted)
        )
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

    public async Task<int> AddAsync(FeatureEdit feature)
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
                    ToAttachmentData
                )
                .ToArray()
        };

        var addedFeature = await dbContext.Features.AddAsync(featureData);
        await dbContext.SaveChangesAsync();

        return addedFeature.Entity.Id;
    }

    private static AttachmentData ToAttachmentData(Attachment a) => new()
    {
        Name = a.Name,
        Extension = a.Extension,
        Size = a.Size,
        FileId = a.FileId
    };

    private static SubTaskData ToSubTaskData(SubTaskEdit t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        ExecutorEmployeeId = t.ExecutorEmployeeId,
        Deadline = t.Deadline,
        Cost = t.Cost,
        IsCompleted = t.IsCompleted,
        OnPayment = t.OnPayment
    };

    public async Task EditAsync(FeatureEdit feature)
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

        HandleSubTasks(feature, featureData);
        
        var existingTasks = featureData.AttachmentsList
            .ToDictionary(t => t.Id);

        foreach (var attachment in feature.AttachmentsList)
        {
            if (attachment.Id == Guid.Empty)
            {
                featureData.AttachmentsList.Add(
                    ToAttachmentData(attachment)
                );

                continue;
            }

            existingTasks.Remove(attachment.Id, out AttachmentData _);
        }

        if (existingTasks.Count > 0)
        {
            dbContext.Attachments.RemoveRange(existingTasks.Values);
        }

        await dbContext.SaveChangesAsync();
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
            existingTask.IsCompleted = taskEdit.IsCompleted;
            existingTask.OnPayment = taskEdit.OnPayment;

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

    private static Expression<Func<FeatureData, Feature>> GetFeatureMapExpression(
        Guid? subTaskEmployeeId
    ) => f => new Feature
    {
        Id = f.Id,
        Name = f.Name,
        Description = f.Description,
        Project = new Project
        {
            Id = f.Project.Id,
            Name = f.Project.Name
        },
        TasksList = f.TasksList
            .Where(t => subTaskEmployeeId == null || t.ExecutorEmployeeId == subTaskEmployeeId)
            .OrderBy(t => t.Deadline)
            .Select(
                t => new SubTask
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
                        Email = t.ExecutorEmployee.Email
                    }
                }
            )
            .ToArray(),
        AttachmentsList = f.AttachmentsList
            .Select(
                a => new Attachment
                {
                    Id = a.Id,
                    Name = a.Name,
                    Extension = a.Extension,
                    Size = a.Size,
                    FileId = a.FileId
                }
            )
            .ToArray()
    };
}