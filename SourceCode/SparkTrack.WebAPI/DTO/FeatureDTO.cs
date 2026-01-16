namespace SparkTrack.WebAPI.DTO;

public record FeatureDTO
{
    public int Id { get; init; }
    
    public required string Name { get; init; }
    
    public required ProjectDTO Project { get; init; }
    
    public required IReadOnlyList<SubTaskDTO> TasksList { get; init; }

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<AttachmentDTO> AttachmentsList { get; init; } = [];
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? EditedAt { get; init; }
    
    public Guid Version { get; init; }
}