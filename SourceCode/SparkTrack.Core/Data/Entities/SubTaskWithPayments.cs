namespace SparkTrack.Core.Data.Entities;

using Shared.Data.Entities;

public record SubTaskWithPayments : SubTask
{
    public IReadOnlyList<PaymentInfo> Payments { get; init; } = [];
    
    public float RemainingMainPayment { get; init; }
    
    public float RemainingTimelyBonusPayment { get; init; }
}