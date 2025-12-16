namespace SparkTrack.WebAPI.DTO;

public record FileInfoDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Link { get; init; }
}