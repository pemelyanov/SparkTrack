namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Edit;
using DTO.Edit;

public static class FeatureMappingExtensions
{
    public static FeatureEditDTO ToDTO(this FeatureEdit it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ProjectId = it.ProjectId,
        TasksList = it.TasksList.Select(task => task.ToDTO()).ToArray(),
        Deadline = it.Deadline,
        Description = it.Description,
        AttachmentsIdList = it.AttachmentsIdList
    };

    public static FeatureEdit ToDomain(this FeatureEditDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ProjectId = it.ProjectId,
        TasksList = it.TasksList.Select(task => task.ToDomain()).ToArray(),
        Deadline = it.Deadline,
        Description = it.Description,
        AttachmentsIdList = it.AttachmentsIdList
    };
}