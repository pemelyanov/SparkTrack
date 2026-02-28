namespace SparkTrack.AvaloniaImpl.Data.Templates;

public record SubTaskTemplate : ITemplate
{
    public Guid TaskId { get; init; }
    
    public string TemplateName { get; set; } = string.Empty;
    
    public required string Name { get; init; }
    
    public UserSelectionTemplate? ExecutorEmployee { get; init; }

    public IReadOnlyList<Guid> DependsOnIdList { get; init; } = [];
    
    public required TimeSpan Deadline { get; init; }
    
    public float Cost { get; init; }
    
    public float TimelyBonus { get; init; }
}