namespace SparkTrack.WebAPI.DTO;

public record LogInDTO
{
    public required string Email { get; init; }
    
    public required string Password { get; init; }
}