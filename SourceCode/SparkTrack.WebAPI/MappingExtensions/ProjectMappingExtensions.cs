namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Entities;
using DTO;

public static class ProjectMappingExtensions
{
    public static ProjectDTO ToDTO(this Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Link = project.Link,
        ArchivedAt = project.ArchivedAt,
        ArchiveSource = project.ArchiveSource
    };
    
    public static Project ToDomain(this ProjectDTO project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Link = project.Link,
        ArchivedAt = project.ArchivedAt,
        ArchiveSource = project.ArchiveSource
    };
}