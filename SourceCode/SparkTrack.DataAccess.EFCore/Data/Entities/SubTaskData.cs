namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record SubTaskData
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }

    public UserData ExecutorEmployee { get; init; } = null!;
    
    public required Guid ExecutorEmployeeId { get; init; }
    
    public float Cost { get; init; }
    
    public bool IsCompleted { get; init; }
    
    public bool OnPayment { get; init; }
}