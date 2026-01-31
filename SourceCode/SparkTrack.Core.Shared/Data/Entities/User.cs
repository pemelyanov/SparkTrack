namespace SparkTrack.Core.Shared.Data.Entities;

using Enums;

public record User
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required ERole Role { get; init; }
    
    public required string Email { get; init; }
    
    public string? TelegramTag { get; init; }
    
    public string? PasswordHash { get; init; }

    public static User Empty(Guid id) => new()
    {
        Id = id,
        Email = string.Empty,
        Name = string.Empty,
        Role = ERole.None
    };
}