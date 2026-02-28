namespace SparkTrack.DataAccess.EFCore.Data.Entities;

using Core.Shared.Enums;

public record FeatureData : IAttachmentsOwner
{
    public int Id { get; init; }
    
    public required string Name { get; set; }

    public ProjectData Project { get; set; } = null!;
    
    public Guid ProjectId { get; set; }

    public ICollection<SubTaskData> TasksList { get; init; } = [];

    public string Description { get; set; } = string.Empty;

    public ICollection<AttachmentData> AttachmentsList { get; init; } = [];
    
    public ICollection<UserData> AuthorsList { get; init; } = [];
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? EditedAt { get; set; }
    
    public Guid Version { get; set; }
    
    public DateTime? ArchivedAt { get; set; }
    
    public EArchiveSource? ArchiveSource { get; set; }
}