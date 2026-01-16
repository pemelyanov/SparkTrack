namespace SparkTrack.WebAPI.DTO.Edit;

using Core.Shared.Data.Entities;

public record FeatureEditDTO
{
    public int Id { get; init; }
    
    public required string Name { get; init; }
    
    public required Guid ProjectId { get; init; }
    
    public required IReadOnlyList<SubTaskEditDTO> TasksList { get; init; }

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<AttachmentDTO> AttachmentsList { get; init; } = [];
    
    public Guid Version { get; init; }
}