namespace SparkTrack.Core.Shared.Data.Edit;

using Entities;

public record FeatureEdit
{
    public int Id { get; init; }
    
    public required string Name { get; init; }
    
    public required Guid ProjectId { get; init; }
    
    public required IReadOnlyList<SubTaskEdit> TasksList { get; init; }
    
    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<Attachment> AttachmentsList { get; init; } = [];
}