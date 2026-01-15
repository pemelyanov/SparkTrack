namespace SparkTrack.Core.Shared.Data.Entities;

public record Comment
{
    public Guid Id { get; init; }
    
    public required User Author { get; init; }

    public string Text { get; init; } = string.Empty;

    public IReadOnlyList<Attachment> AttachmentsList { get; init; } = [];
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? EditedAt { get; init; }
}