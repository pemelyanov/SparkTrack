namespace SparkTrack.Core.Shared.Data.Edit;

using Enums;

public record SubTaskEdit
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required Guid ExecutorEmployeeId { get; init; }
    
    public required DateTime Deadline { get; init; }
    
    public float Cost { get; init; }
    
    public bool IsCompleted { get; init; }
    
    public EPaymentStatus PaymentStatus { get; init; }
    
    public Guid Version { get; init; }
}