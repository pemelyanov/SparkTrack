namespace SparkTrack.API.MappingExtensions;

using API;
using Core.Shared.Data.Entities;

public static class ProjectMappingExtensions
{
    public static ProjectDTO ToDTO(this Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Link = project.Link
    };
    
    public static Project ToDomain(this ProjectDTO project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Link = project.Link
    };
}