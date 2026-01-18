namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using DTO;
using DTO.Edit;

public static class UserMappingExtensions
{
    public static UserDTO ToDTO(this User it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        Role = it.Role,
        Email = it.Email,
        TelegramTag = it.TelegramTag
    };
    
    public static UserEdit ToDomain(this UserEditDTO it) => new()
    {
        Name = it.Name,
        Email = it.Email,
        TelegramTag = it.TelegramTag
    };
}