namespace SparkTrack.WebAPI.DTO;

using Core.Shared.Enums;

public record UserDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required ERole Role { get; init; }
    
    public required string Email { get; init; }
    
    public string? TelegramTag { get; init; }
    
    public required DateTime? ArchivedAt { get; init; }
    
    public required EArchiveSource? ArchiveSource { get; init; }
}