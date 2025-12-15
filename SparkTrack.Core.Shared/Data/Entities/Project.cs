namespace SparkTrack.Core.Shared.Data.Entities;

public record Project
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public string? Link { get; init; }
}