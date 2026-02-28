namespace SparkTrack.API.MappingExtensions;

using API;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using EArchiveSource = Core.Shared.Enums.EArchiveSource;

public static class FeatureMappingExtensions
{
    public static FeatureEditDTO ToDTO(this FeatureEdit it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ProjectId = it.ProjectId,
        TasksList = it.TasksList.Select(task => task.ToDTO()).ToArray(),
        Description = it.Description,
        AttachmentsList = it.AttachmentsList.Select(a => a.ToDTO()).ToArray(),
        Version = it.Version,
        AuthorsIdList = it.AuthorsIdList
    };
    
    public static Feature ToDomain(this FeatureDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        Project = it.Project.ToDomain(),
        TasksList = it.TasksList.Select(task => task.ToDomain()).ToArray(),
        Description = it.Description,
        AttachmentsList = it.AttachmentsList.Select(file => file.ToDomain()).ToArray(),
        CreatedAt = it.CreatedAt.ToLocalTime(),
        EditedAt = it.EditedAt?.ToLocalTime(),
        Version = it.Version,
        ArchivedAt = it.ArchivedAt,
        ArchiveSource = it.ArchiveSource?.Cast<EArchiveSource>(),
        AuthorsList = it.AuthorsList.Select(a => a.ToDomain()).ToArray()
    };
}