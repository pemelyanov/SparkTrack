namespace SparkTrack.WebAPI.DTO;

public record PaymentDetailsDTO : PaymentDTO
{
    public required SubTaskDTO Task { get; init; }
    
    public required ProjectDTO Project { get; init; }
}