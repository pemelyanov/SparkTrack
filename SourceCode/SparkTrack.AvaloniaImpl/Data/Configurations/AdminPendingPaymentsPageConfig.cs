namespace SparkTrack.AvaloniaImpl.Data.Configurations;

public struct AdminPendingPaymentsPageConfig() : IColumnsConfig
{
    public Guid? ProjectId { get; init; }
    
    public Guid? EmployeeId { get; init; }
    
    public bool? IsDatesFilterEnabled { get; init; }
    
    public DateTime? StartDate { get; init; }
    
    public DateTime? EndDate { get; init; }
    
    public bool? ShowOnlyMine { get; init; }
    
    public bool? ShowPaid { get; init; }
    
    public int? ItemsPerPage { get; init; }
    
    public Dictionary<string, double> ColumnWidths { get; init; } = [];
}