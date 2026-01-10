namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record FeatureData
{
    public int Id { get; init; }
    
    public required string Name { get; set; }

    public ProjectData Project { get; set; } = null!;
    
    public Guid ProjectId { get; set; }

    public ICollection<SubTaskData> TasksList { get; init; } = [];

    public string Description { get; set; } = string.Empty;

    public ICollection<FileData> AttachmentsList { get; init; } = [];
}