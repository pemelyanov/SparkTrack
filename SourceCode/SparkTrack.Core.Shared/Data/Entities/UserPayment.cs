namespace SparkTrack.Core.Shared.Data.Entities;

public record UserPayment
{
    public required User User { get; init; }
    
    public float Payment { get; init; }
}