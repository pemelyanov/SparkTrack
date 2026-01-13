namespace SparkTrack.Core.Shared.Data.Entities;

public record Attachment
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Extension { get; init; }
    
    public long Size { get; init; }
    
    public Guid FileId { get; init; }
}