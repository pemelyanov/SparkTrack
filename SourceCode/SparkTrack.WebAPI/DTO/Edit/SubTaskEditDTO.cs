namespace SparkTrack.WebAPI.DTO.Edit;

public record SubTaskEditDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required Guid ExecutorEmployeeId { get; init; }
    
    public required DateTime Deadline { get; init; }
    
    public float Cost { get; init; }
    
    public bool IsCompleted { get; init; }
    
    public bool OnPayment { get; init; }
}