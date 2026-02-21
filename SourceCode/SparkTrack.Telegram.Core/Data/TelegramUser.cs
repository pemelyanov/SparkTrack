namespace SparkTrack.Telegram.Core.Data;

public record TelegramUser
{
    public Guid UserId { get; init; }
    
    public long ChatId { get; init; }
    
    public bool IsNotificationsEnabled { get; init; }
    
    public bool IsHelloSent { get; init; }
    
    public TimeSpan? TimeZone { get; init; }
}