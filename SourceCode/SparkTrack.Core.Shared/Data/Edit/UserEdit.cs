namespace SparkTrack.Core.Shared.Data.Edit;

public record UserEdit
{
    public required string Name { get; init; }
    
    public required string Email { get; init; }
}