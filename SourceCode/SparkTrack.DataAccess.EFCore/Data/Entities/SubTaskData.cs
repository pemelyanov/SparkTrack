namespace SparkTrack.DataAccess.EFCore.Data.Entities;

using Core.Shared.Data.Entities;
using Core.Shared.Enums;

public record SubTaskData
{
    public Guid Id { get; init; }
    
    public required string Name { get; set; }

    public UserData ExecutorEmployee { get; set; } = null!;
    
    public required Guid ExecutorEmployeeId { get; set; }
    
    public required DateTime Deadline { get; set; }

    public FeatureData Feature { get; set; } = null!;

    public ICollection<PaymentData> Payments { get; } = [];
    
    public float Cost { get; set; }
    
    public float TimelyBonus { get; set; }
    
    public bool IsTimelyBonusApproved { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public EPaymentStatus PaymentStatus { get; set; }
    
    public Guid Version { get; set; }
}