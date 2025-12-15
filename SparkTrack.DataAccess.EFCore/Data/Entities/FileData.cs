namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record FileData
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Link { get; init; }
}