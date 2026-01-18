namespace SparkTrack.Core.Shared.Data.Entities;

public record UserRemainingPayment
{
    public required User User { get; init; }
    
    public required Project Project { get; init; }
    
    public float RemainingPayment { get; init; }
}