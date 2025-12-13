namespace SparkTrack.Core.Data.Entities;

using Enums;

public record User
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required ERole Role { get; init; }
}