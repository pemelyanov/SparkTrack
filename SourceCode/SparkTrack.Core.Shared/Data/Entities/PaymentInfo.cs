namespace SparkTrack.Core.Shared.Data.Entities;

using Enums;

public record PaymentInfo
{
    public Guid Id { get; init; }
    
    public Guid TaskId { get; init; }
    
    public required User Admin { get; init; }
    
    public float Payment { get; init; }
    
    public EPaymentType PaymentType { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}