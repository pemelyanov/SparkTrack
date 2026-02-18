namespace SparkTrack.AvaloniaImpl.Data.Templates;

public record TemplateGroup<TTemplate> where TTemplate : ITemplate
{
    public required string Name { get; init; }

    public IReadOnlyList<TTemplate> Templates { get; init; } = [];
}