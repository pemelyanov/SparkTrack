namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public record BonusPaymentData
{
    public Guid Id { get; init; }
    
    public Guid EmployeeId { get; set; }
    
    public Guid AdminId { get; set; }

    public UserData Admin { get; set; } = null!;
    
    public string? Comment { get; set; }
    
    public float Payment { get; set; }
    
    public DateTime CreatedAt { get; set; }
}