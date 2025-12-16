namespace SparkTrack.DataAccess.EFCore.Repositories;

using System.Linq.Expressions;
using Core.Repositories;
using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Data.Entities;
using Extensions;
using Microsoft.EntityFrameworkCore;

internal class FeaturesRepository(SparkTrackDbContext dbContext) : IFeaturesRepository
{
    public Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        Guid? projectId,
        bool showCompleted,
        Guid? subTaskEmployeeId,
        PageQuery pageQuery
    ) => dbContext.Features
        .AsNoTracking()
        .WhereIf(projectId is not null, it => it.Project.Id == projectId)
        .WhereIf(
            subTaskEmployeeId is not null,
            it => it.TasksList.Any(task => task.ExecutorEmployee.Id == subTaskEmployeeId)
        )
        .Select(
            GetFeatureMapExpression(subTaskEmployeeId)
        )
        .AsPaginated(pageQuery)
        .CollectAsync();

    public Task<Feature?> GetAsync(int id, Guid? subTaskEmployeeId) => dbContext.Features.Where(it => it.Id == id)
        .Select(GetFeatureMapExpression(subTaskEmployeeId))
        .FirstOrDefaultAsync();

    public async Task AddAsync(FeatureEdit feature)
    {
        var subTasks = feature.TasksList.Select(
                it => new SubTaskData
                {
                    Id = it.Id,
                    Name = it.Name,
                    ExecutorEmployeeId = it.ExecutorEmployeeId,
                    Cost = it.Cost,
                    IsCompleted = it.IsCompleted,
                    OnPayment = it.OnPayment
                }
            )
            .ToArray();

        // var attachments = dbContext.Files.Where(it => feature.AttachmentsIdList.Any(id => it.Id == id)).ToArray();

        var featureData = new FeatureData
        {
            Name = feature.Name,
            ProjectId = feature.ProjectId,
            TasksList = subTasks,
            Deadline = feature.Deadline,
            Description = feature.Description,
        };

        await dbContext.SubTasks.AddRangeAsync(subTasks);
        await dbContext.Features.AddAsync(featureData);
        
        await dbContext.SaveChangesAsync();
    }

    public Task DeleteAsync(int id) => throw new NotImplementedException();

    private Expression<Func<FeatureData, Feature>> GetFeatureMapExpression(Guid? subTaskEmployeeId) => it => new Feature
    {
        Id = it.Id,
        Name = it.Name,
        Project = new Project
        {
            Id = it.Project.Id,
            Name = it.Project.Name
        },
        TasksList = it.TasksList
            .Where(task => subTaskEmployeeId == null || task.ExecutorEmployee.Id == subTaskEmployeeId)
            .Select(
                task => new SubTask
                {
                    Name = task.Name,
                    Id = task.Id,
                    ExecutorEmployee = new User
                    {
                        Id = task.ExecutorEmployee.Id,
                        Name = task.ExecutorEmployee.Name,
                        Role = task.ExecutorEmployee.Role
                    }
                }
            )
            .ToArray(),
        Deadline = it.Deadline,
        Description = it.Description,
        AttachmentsList = it.AttachmentsList.Select(
                attachment => new FileInfo
                {
                    Id = attachment.Id,
                    Name = attachment.Name,
                    Link = attachment.Link
                }
            )
            .ToArray()
    };
}