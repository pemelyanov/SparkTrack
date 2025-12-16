namespace SparkTrack.WebAPI.DTO;

public record ProjectDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public string? Link { get; init; }
}