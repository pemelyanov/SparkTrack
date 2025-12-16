namespace SparkTrack.WebAPI.DTO;

public record SubTaskDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required UserDTO ExecutorEmployee { get; init; }
    
    public float Cost { get; init; }
    
    public bool IsCompleted { get; init; }
    
    public bool OnPayment { get; init; }
}