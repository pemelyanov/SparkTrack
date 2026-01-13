namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Entities;
using DTO;

public static class AttachmentMappingExtensions
{
    public static AttachmentDTO ToDTO(this Attachment attachment) => new()
    {
        Id = attachment.Id,
        Name = attachment.Name,
        Extension = attachment.Extension,
        Size = attachment.Size,
        FileId = attachment.FileId,
        IsImage = attachment.IsImage,
        Checksum = attachment.Checksum
    };
    
    public static Attachment ToDomain(this AttachmentDTO attachment) => new()
    {
        Id = attachment.Id,
        Name = attachment.Name,
        Extension = attachment.Extension,
        Size = attachment.Size,
        FileId = attachment.FileId,
        IsImage = attachment.IsImage,
        Checksum = attachment.Checksum
    };
}