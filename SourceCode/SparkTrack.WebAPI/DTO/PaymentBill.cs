namespace SparkTrack.WebAPI.DTO;

public record PaymentBillDTO
{
    public required FeatureDTO Feature { get; init; }
    
    public required SubTaskDTO SubTask { get; init; }
}