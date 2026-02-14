namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Exceptions;
using Core.Repositories;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

internal class ProjectsRepository(SparkTrackDbContext dbContext) : IProjectsRepository
{
    public async Task<IReadOnlyList<Project>> GetListAsync(Guid? userId = null)
    {
        IQueryable<ProjectData> projectsQuery;

        if (userId is null) projectsQuery = dbContext.Projects.AsNoTracking();
        else
        {
            var projectIdsList = await dbContext.Features
                .AsNoTracking()
                .Where(it => it.TasksList.Any(task => task.ExecutorEmployeeId == userId))
                .Select(it => it.ProjectId)
                .Distinct()
                .ToArrayAsync();

            projectsQuery = dbContext.Projects.AsNoTracking().Where(it => projectIdsList.Contains(it.Id));
        }

        return await projectsQuery
            // TODO: Add filter
            .Where(it => it.ArchivedAt == null)
            .Select(it => new Project
                {
                    Id = it.Id,
                    Name = it.Name,
                    Link = it.Link,
                    ArchivedAt = it.ArchivedAt,
                    ArchiveSource = it.ArchiveSource
                }
            )
            .ToArrayAsync();
    }

    public async Task AddAsync(Project project)
    {
        var projectData = new ProjectData
        {
            Id = project.Id,
            Name = project.Name,
            Link = project.Link
        };

        await dbContext.Projects.AddAsync(projectData);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        var projectData = await dbContext.Projects.FindAsync(project.Id);

        if (projectData is null)
        {
            throw new NotFoundException($"Project with id {project.Id} not found");
        }

        projectData.Name = project.Name;
        projectData.Link = projectData.Link;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await dbContext.Projects.FindAsync(id);

        if (project is null) return;

        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync();
    }

    public async Task SetArchiveStatus(Guid id, bool isArchived, EArchiveSource? archiveSource = null)
    {
        var project = await dbContext.Projects.FindAsync(id);

        if (project is null) return;

        project.ArchiveSource =
            isArchived ? archiveSource ?? throw new InvalidOperationException("Enter archive source") : null;

        project.ArchivedAt = isArchived ? DateTime.UtcNow : null;

        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> HasRelationsAsync(Guid id)
    {
        var project = await dbContext.Projects.AsNoTracking()
            .Where(it => it.Id == id)
            .Include(it => it.Features)
            .ThenInclude(featureData => featureData.TasksList)
            .ThenInclude(task => task.Payments)
            .FirstOrDefaultAsync();

        if (project is null) return false;

        return project.Features.Any(f => f.TasksList.Any(t => t.Payments.Any()));
    }
}