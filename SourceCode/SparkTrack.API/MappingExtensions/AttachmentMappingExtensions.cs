namespace SparkTrack.API.MappingExtensions;

using API;
using Core.Shared.Data.Entities;

public static class AttachmentMappingExtensions
{
    public static AttachmentDTO ToDTO(this Attachment attachment) => new()
    {
        Id = attachment.Id,
        Name = attachment.Name,
        Extension = attachment.Extension,
        Size = attachment.Size,
        FileId = attachment.FileId
    };
    
    public static Attachment ToDomain(this AttachmentDTO attachment) => new()
    {
        Id = attachment.Id,
        Name = attachment.Name,
        Extension = attachment.Extension,
        Size = attachment.Size,
        FileId = attachment.FileId
    };
}