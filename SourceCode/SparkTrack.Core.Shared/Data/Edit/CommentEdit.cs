namespace SparkTrack.Core.Shared.Data.Edit;

using Entities;

public class CommentEdit
{
    public Guid Id { get; init; }

    public string Text { get; init; } = string.Empty;

    public IReadOnlyList<Attachment> AttachmentsList { get; init; } = [];
}