namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Exceptions;
using Core.Repositories;
using Core.Shared.Data.Entities;
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

        return await projectsQuery.Select(
                it => new Project
                {
                    Id = it.Id,
                    Name = it.Name,
                    Link = it.Link
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

    public Task DeleteAsync(Guid id) => throw new NotImplementedException();
}