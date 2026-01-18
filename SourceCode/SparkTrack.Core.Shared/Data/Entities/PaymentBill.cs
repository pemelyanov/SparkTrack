namespace SparkTrack.Core.Shared.Data.Entities;

public record PaymentBill
{
    public required Feature Feature { get; init; }
    
    public required SubTask SubTask { get; init; }
}