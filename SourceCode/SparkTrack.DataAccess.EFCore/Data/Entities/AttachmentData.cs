namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record AttachmentData
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Extension { get; init; }
    
    public long Size { get; init; }
    
    public Guid FileId { get; init; }
    
    public bool IsImage { get; init; }
    
    public byte[] Checksum { get; init; } = [];
}