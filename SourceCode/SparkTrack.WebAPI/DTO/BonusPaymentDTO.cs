namespace SparkTrack.WebAPI.DTO;

public record BonusPaymentDTO
{
    public Guid Id { get; init; }
    
    public Guid EmployeeId { get; init; }
    
    public required UserDTO Admin { get; init; }
    
    public string? Comment { get; init; }
    
    public float Payment { get; init; }
}