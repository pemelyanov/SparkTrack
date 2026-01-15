namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record CommentData : IAttachmentsOwner
{
    public Guid Id { get; init; }

    public UserData User { get; set; } = null!;
    
    public Guid UserId { get; set; }

    public FeatureData Feature { get; set; } = null!;
    
    public int FeatureId { get; set; }

    public string Text { get; set; } = string.Empty;

    public ICollection<AttachmentData> AttachmentsList { get; init; } = [];
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? EditedAt { get; set; }
}