using SparkTrack.Core.Shared.Data;

namespace SparkTrack.AvaloniaImpl.Data.Configurations;

public struct FeaturesPageConfig() : IColumnsConfig
{
    public bool? ShowOnlyMine { get; init; }

    public bool? IsDatesFilterEnabled { get; init; }
    
    public FeatureFilterQuery? Filters { get; init; }
    
    public int? ItemsPerPage { get; init; }
    
    public SortQuery? Sort { get; init; }

    public Dictionary<string, double> ColumnWidths { get; init; } = [];
}