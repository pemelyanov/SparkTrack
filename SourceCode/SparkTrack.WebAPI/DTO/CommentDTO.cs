namespace SparkTrack.WebAPI.DTO;

public record CommentDTO
{
    public Guid Id { get; init; }
    
    public required UserDTO Author { get; init; }

    public string Text { get; init; } = string.Empty;

    public IReadOnlyList<AttachmentDTO> AttachmentsList { get; init; } = [];
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? EditedAt { get; init; }
}