namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Entities;
using DTO;

public static class FileInfoMappingExtensions
{
    public static FileInfoDTO ToDTO(this FileInfo fileInfo) => new()
    {
        Id = fileInfo.Id,
        Name = fileInfo.Name,
        Link = fileInfo.Link
    };
}