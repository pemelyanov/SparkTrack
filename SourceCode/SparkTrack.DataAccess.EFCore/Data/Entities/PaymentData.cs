namespace SparkTrack.DataAccess.EFCore.Data.Entities;

using Core.Shared.Enums;

public record PaymentData
{
    public Guid Id { get; init; }
    
    public Guid TaskId { get; set; }
    
    public Guid AdminId { get; set; }

    public UserData Admin { get; set; } = null!;
    
    public EPaymentType PaymentType { get; set; }
    
    public float Payment { get; set; }
    
    public DateTime CreatedAt { get; set; }
}