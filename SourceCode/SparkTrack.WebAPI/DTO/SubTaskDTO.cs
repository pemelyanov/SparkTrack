namespace SparkTrack.WebAPI.DTO;

using Core.Shared.Enums;

public record SubTaskDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required UserDTO ExecutorEmployee { get; init; }
    
    public required DateTime Deadline { get; init; }
    
    public float Cost { get; init; }
    
    public float TimelyBonus { get; init; }
    
    public bool IsTimelyBonusApproved { get; init; }
    
    public IReadOnlyList<Guid> DependsOnIdList { get; init; } = [];
    
    public DateTime? CompletedAt { get; init; }
    
    public bool IsCompleted { get; init; }
    
    public EPaymentStatus PaymentStatus { get; init; }
    
    public Guid Version { get; init; }
}