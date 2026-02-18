namespace SparkTrack.AvaloniaImpl.Data.Templates;

public record SubTaskTemplate(string TemplateName) : ITemplate
{
    public required string Name { get; init; }
    
    public required UserSelectionTemplate ExecutorEmployee { get; init; }
    
    public required TimeSpan Deadline { get; init; }
    
    public float Cost { get; init; }
    
    public float TimelyBonus { get; init; }
}