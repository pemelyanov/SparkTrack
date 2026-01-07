namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record SubTaskData
{
    public Guid Id { get; init; }
    
    public required string Name { get; set; }

    public UserData ExecutorEmployee { get; set; } = null!;
    
    public required Guid ExecutorEmployeeId { get; set; }
    
    public float Cost { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public bool OnPayment { get; set; }
}