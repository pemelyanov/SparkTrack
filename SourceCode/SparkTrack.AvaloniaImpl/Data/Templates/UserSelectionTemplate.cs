namespace SparkTrack.AvaloniaImpl.Data.Templates;

public record UserSelectionTemplate
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
}