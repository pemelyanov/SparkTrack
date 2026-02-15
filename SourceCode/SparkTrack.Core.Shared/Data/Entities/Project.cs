namespace SparkTrack.Core.Shared.Data.Entities;

using Enums;

public record Project
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public string? Link { get; init; }
    
    public DateTime? ArchivedAt { get; init; }
    
    public EArchiveSource? ArchiveSource { get; init; }
}