namespace SparkTrack.WebAPI.Services.TelegramMessageSender;

public interface ITelegramMessageSender
{
    Task SendAsync(string userTag, string message);
}