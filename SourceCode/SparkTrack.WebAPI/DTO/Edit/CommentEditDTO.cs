namespace SparkTrack.WebAPI.DTO.Edit;

public record CommentEditDTO
{
    public Guid Id { get; init; }

    public string Text { get; init; } = string.Empty;

    public IReadOnlyList<AttachmentDTO> AttachmentsList { get; init; } = [];
}