namespace SparkTrack.Core.Shared.Data.Entities;

public record PaymentDetails : PaymentInfo
{
    public required SubTask Task { get; init; }
    
    public required Project Project { get; init; }
}