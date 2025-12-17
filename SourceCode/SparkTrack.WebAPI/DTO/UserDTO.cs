namespace SparkTrack.WebAPI.DTO;

using Core.Shared.Enums;

public record UserDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required ERole Role { get; init; }
    
    public required string Email { get; init; }
}