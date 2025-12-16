namespace SparkTrack.API.MappingExtensions;

using API;
using Core.Shared.Data.Entities;

public static class FileInfoMappingExtensions
{
    public static FileInfoDTO ToDTO(this FileInfo fileInfo) => new()
    {
        Id = fileInfo.Id,
        Name = fileInfo.Name,
        Link = fileInfo.Link
    };
    
    public static FileInfo ToDomain(this FileInfoDTO fileInfo) => new()
    {
        Id = fileInfo.Id,
        Name = fileInfo.Name,
        Link = fileInfo.Link
    };
}