namespace SparkTrack.API.MappingExtensions;

using API;
using Core.Shared.Data.Entities;

public static class UserMappingExtensions
{
    public static UserDTO ToDTO(this User it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        Role = it.Role.Cast<ERole>()
    };
    
    public static User ToDomain(this UserDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        Role = it.Role.Cast<Core.Shared.Enums.ERole>()
    };
}