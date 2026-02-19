namespace SparkTrack.AvaloniaImpl.Data.Templates;

public record TemplateGroup<TTemplate> : ITemplateGroup where TTemplate : ITemplate
{
    public required string Name { get; init; }

    IReadOnlyList<ITemplate> ITemplateGroup.Templates => (IReadOnlyList<ITemplate>)Templates;
    
    public IReadOnlyList<TTemplate> Templates { get; init; } = [];
}

public interface ITemplateGroup
{
    public string Name { get; }

    public IReadOnlyList<ITemplate> Templates { get; }
}