namespace SparkTrack.Core.Shared.Data.Entities;

public record SubTask
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required User ExecutorEmployee { get; init; }
    
    public required DateTime Deadline { get; init; }
    
    public float Cost { get; init; }
    
    public bool IsCompleted { get; init; }
    
    public bool OnPayment { get; init; }
}