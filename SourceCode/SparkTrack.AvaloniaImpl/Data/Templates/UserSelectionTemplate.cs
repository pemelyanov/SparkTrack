namespace SparkTrack.AvaloniaImpl.Data.Templates;

public record UserSelectionTemplate(string TemplateName) : ITemplate
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
}