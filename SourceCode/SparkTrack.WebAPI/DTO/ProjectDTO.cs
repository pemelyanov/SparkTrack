namespace SparkTrack.WebAPI.DTO;

using Core.Shared.Enums;

public record ProjectDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public string? Link { get; init; }
    
    public required DateTime? ArchivedAt { get; init; }
    
    public required EArchiveSource? ArchiveSource { get; init; }
}