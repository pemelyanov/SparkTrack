namespace SparkTrack.API.MappingExtensions;

using SparkTrack.Core.Shared.Data.Edit;
using SparkTrack.Core.Shared.Data.Entities;

public static class CommentMappingExtensions
{
    public static CommentEditDTO ToDTO(this CommentEdit it) => new()
    {
        Id = it.Id,
        Text = it.Text,
        AttachmentsList = it.AttachmentsList.Select(a => a.ToDTO()).ToArray()
    };

    public static CommentEdit ToDomain(this CommentEditDTO it) => new()
    {
        Id = it.Id,
        Text = it.Text,
        AttachmentsList = it.AttachmentsList.Select(a => a.ToDomain()).ToArray()
    };
    
    public static Comment ToDomain(this CommentDTO it) => new()
    {
        Id = it.Id,
        Text = it.Text,
        Author = it.Author.ToDomain(),
        CreatedAt = it.CreatedAt.ToLocalTime(),
        EditedAt = it.EditedAt?.ToLocalTime(),
        AttachmentsList = it.AttachmentsList.Select(a => a.ToDomain()).ToArray()
    };
}