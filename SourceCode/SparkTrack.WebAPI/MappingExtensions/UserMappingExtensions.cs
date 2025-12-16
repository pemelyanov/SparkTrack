namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Entities;
using DTO;

public static class UserMappingExtensions
{
    public static UserDTO ToDTO(this User it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        Role = it.Role
    };
}