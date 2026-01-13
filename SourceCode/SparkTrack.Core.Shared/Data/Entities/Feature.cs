namespace SparkTrack.Core.Shared.Data.Entities;

public record Feature
{
    public int Id { get; init; }
    
    public required string Name { get; init; }
    
    public required Project Project { get; init; }
    
    public required IReadOnlyList<SubTask> TasksList { get; init; }

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<AttachmentInfo> AttachmentsList { get; init; } = [];
}