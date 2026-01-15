namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using DTO;
using DTO.Edit;

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
    
    public static CommentDTO ToDTO(this Comment it) => new()
    {
        Id = it.Id,
        Text = it.Text,
        Author = it.Author.ToDTO(),
        CreatedAt = it.CreatedAt,
        EditedAt = it.EditedAt,
        AttachmentsList = it.AttachmentsList.Select(a => a.ToDTO()).ToArray()
    };
}