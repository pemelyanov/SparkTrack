namespace SparkTrack.Core.Shared.Data.Entities;

public record Payment
{
    public Guid Id { get; init; }
    
    public Guid TaskId { get; init; }
    
    public Guid AdminId { get; init; }
    
    public float MainPayment { get; init; }
    
    public float TimelyBonusPayment { get; init; }
}