namespace SparkTrack.Core.Data;

using Enums;

public record User
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required ERole Role { get; init; }
}