namespace SparkTrack.DataAccess.EFCore.Data.Entities;

using Core.Shared.Data.Entities;

public record ProjectData
{
    public Guid Id { get; init; }
    
    public required string Name { get; set; }
    
    public string? Link { get; set; }

    public ICollection<FeatureData> Features { get; } = [];
}