namespace SparkTrack.WebAPI.DTO;

public record UserPaymentDTO
{
    public required UserDTO User { get; init; }
    
    public float RemainingPayment { get; init; }
}