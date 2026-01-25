namespace SparkTrack.WebAPI.DTO;

using Core.Shared.Enums;

public record PaymentDTO
{
    public Guid Id { get; init; }
    
    public Guid TaskId { get; init; }
    
    public required UserDTO Admin { get; init; }
    
    public float Payment { get; init; }
    
    public EPaymentType PaymentType { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}