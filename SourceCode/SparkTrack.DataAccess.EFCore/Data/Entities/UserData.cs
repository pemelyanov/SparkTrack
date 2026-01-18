namespace SparkTrack.DataAccess.EFCore.Data.Entities;

using Core.Shared.Enums;

public record UserData
{
    public Guid Id { get; init; }
    
    public required string Name { get; set; }
    
    public required ERole Role { get; set; }
    
    public required string Email { get; set; }
    
    public string? TelegramTag { get; set; }
    
    public required string PasswordHash { get; set; }
}