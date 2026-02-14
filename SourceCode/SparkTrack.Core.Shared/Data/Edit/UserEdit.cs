namespace SparkTrack.Core.Shared.Data.Edit;

public record UserEdit
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Email { get; init; }
    
    public string? TelegramTag { get; init; }
}