namespace SparkTrack.Telegram.DataAccess.LiteDb.Data;

using Attributes;
using LiteDB;

[CollectionName("telegramUsers")]
public class TelegramUserData
{
    [BsonId]
    public Guid UserId { get; init; }
    
    public long ChatId { get; init; }
    
    public bool IsNotificationsEnabled { get; init; }
    
    public bool IsHelloSent { get; init; }
    
    public TimeSpan? TimeZone { get; init; }
}