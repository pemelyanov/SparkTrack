namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Entities;
using DTO;

public static class FileInfoMappingExtensions
{
    public static FileInfoDTO ToDTO(this AttachmentInfo attachmentInfo) => new()
    {
        Id = attachmentInfo.Id,
        Name = attachmentInfo.Name,
        Link = attachmentInfo.Link
    };
}