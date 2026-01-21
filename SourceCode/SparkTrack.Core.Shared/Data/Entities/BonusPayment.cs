namespace SparkTrack.Core.Shared.Data.Entities;

public record BonusPayment
{
    public Guid Id { get; init; }
    
    public Guid EmployeeId { get; init; }
    
    public Guid AdminId { get; init; }
    
    public string? Comment { get; init; }
    
    public float Payment { get; init; }
}