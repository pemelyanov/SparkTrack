namespace SparkTrack.Core.Data;

public record FileInfo
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Link { get; init; }
}