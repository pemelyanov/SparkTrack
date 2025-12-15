namespace SparkTrack.Core.Shared.Data.Edit;

public record FeatureEdit
{
    public int Id { get; init; }
    
    public required string Name { get; init; }
    
    public required Guid ProjectId { get; init; }
    
    public required IReadOnlyList<SubTaskEdit> TasksList { get; init; }
    
    public DateTime Deadline { get; init; }

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<Guid> AttachmentsIdList { get; init; } = [];
}