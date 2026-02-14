namespace SparkTrack.WebAPI.DTO.Edit;

public record UserEditDTO
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Email { get; init; }
    
    public string? TelegramTag { get; init; }
}