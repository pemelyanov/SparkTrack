namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using DTO;
using DTO.Edit;

public static class FeatureMappingExtensions
{
    public static FeatureEditDTO ToDTO(this FeatureEdit it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ProjectId = it.ProjectId,
        TasksList = it.TasksList.Select(task => task.ToDTO()).ToArray(),
        Description = it.Description,
        AttachmentsIdList = it.AttachmentsIdList
    };

    public static FeatureEdit ToDomain(this FeatureEditDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ProjectId = it.ProjectId,
        TasksList = it.TasksList.Select(task => task.ToDomain()).ToArray(),
        Description = it.Description,
        AttachmentsIdList = it.AttachmentsIdList
    };
    
    public static FeatureDTO ToDTO(this Feature it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        Project = it.Project.ToDTO(),
        TasksList = it.TasksList.Select(task => task.ToDTO()).ToArray(),
        Description = it.Description,
        AttachmentsList = it.AttachmentsList.Select(file => file.ToDTO()).ToArray()
    };
}