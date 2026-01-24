namespace SparkTrack.Core.Shared.Data.Entities;

public record BonusPaymentInfo
{
    public Guid Id { get; init; }
    
    public Guid EmployeeId { get; init; }
    
    public required User Admin { get; init; }
    
    public string? Comment { get; init; }
    
    public float Payment { get; init; }
}