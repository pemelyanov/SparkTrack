namespace SparkTrack.WebAPI.DTO;

public record UserRemainingPaymentDTO
{
    public required UserDTO User { get; init; }
    
    public required ProjectDTO Project { get; init; }
    
    public float RemainingPayment { get; init; }
}