namespace SparkTrack.DataAccess.EFCore.Data.Entities;

using Core.Shared.Data.Entities;

public record CommentData
{
    public Guid Id { get; init; }

    public User User { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public ICollection<Attachment> AttachmentsList { get; init; } = [];
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? EditedAt { get; set; }
}