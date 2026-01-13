namespace SparkTrack.API.MappingExtensions;

using API;
using Core.Shared.Data.Entities;

public static class FileInfoMappingExtensions
{
    public static FileInfoDTO ToDTO(this AttachmentInfo attachmentInfo) => new()
    {
        Id = attachmentInfo.Id,
        Name = attachmentInfo.Name,
        Link = attachmentInfo.Link
    };
    
    public static AttachmentInfo ToDomain(this FileInfoDTO fileInfo) => new()
    {
        Id = fileInfo.Id,
        Name = fileInfo.Name,
        Link = fileInfo.Link
    };
}