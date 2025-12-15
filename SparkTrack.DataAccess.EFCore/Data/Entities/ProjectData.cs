namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record ProjectData
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public string? Link { get; init; }
}