namespace SparkTrack.AvaloniaImpl.Data.Templates;

public class FeatureTemplate : ITemplate
{
    public string TemplateName { get; set; } = string.Empty;
    
    public required string Name { get; init; }

    public IReadOnlyList<SubTaskTemplate> TasksList { get; init; } = [];

    public IReadOnlyList<UserSelectionTemplate> Authors { get; init; } = [];

    public string Description { get; init; } = string.Empty;
}