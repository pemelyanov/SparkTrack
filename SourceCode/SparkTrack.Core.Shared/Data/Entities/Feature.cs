namespace SparkTrack.Core.Shared.Data.Entities;

using Enums;

public record Feature
{
    public int Id { get; init; }
    
    public required string Name { get; init; }
    
    public required Project Project { get; init; }

    public IReadOnlyList<SubTask> TasksList { get; init; } = [];

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<Attachment> AttachmentsList { get; init; } = [];
    
    public ICollection<User> AuthorsList { get; } = [];
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? EditedAt { get; init; }
    
    public Guid Version { get; init; }
    
    public DateTime? ArchivedAt { get; init; }
    
    public EArchiveSource? ArchiveSource { get; init; }
}