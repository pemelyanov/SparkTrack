namespace SparkTrack.API.MappingExtensions;

using API;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;

public static class UserMappingExtensions
{
    public static User ToDomain(this UserDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        Role = it.Role.Cast<Core.Shared.Enums.ERole>(),
        Email = it.Email,
        TelegramTag = it.TelegramTag
    };
    
    public static UserEditDTO ToDTO(this UserEdit it) => new()
    {
        Name = it.Name,
        Email = it.Email,
        TelegramTag = it.TelegramTag
    };
}