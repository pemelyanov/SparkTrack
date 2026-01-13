namespace SparkTrack.Core.Shared.Data.Entities;

public record AttachmentInfo
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Link { get; init; }
}