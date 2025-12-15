namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record FeatureData
{
    public int Id { get; init; }
    
    public required string Name { get; init; }

    public ProjectData Project { get; init; } = null!;
    
    public Guid ProjectId { get; init; }
    
    public required IEnumerable<SubTaskData> TasksList { get; init; }
    
    public DateTime Deadline { get; init; }

    public string Description { get; init; } = string.Empty;

    public IEnumerable<FileData> AttachmentsList { get; init; } = [];
}