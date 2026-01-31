namespace SparkTrack.Core.Shared.Data.Entities;

public record BonusPaymentInfo
{
    public Guid Id { get; init; }
    
    public User Employee { get; init; }
    
    public required User Admin { get; init; }
    
    public string? Comment { get; init; }
    
    public float Payment { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}