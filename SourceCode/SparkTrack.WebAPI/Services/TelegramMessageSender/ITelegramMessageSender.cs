using Telegram.Bot.Types.ReplyMarkups;

namespace SparkTrack.WebAPI.Services.TelegramMessageSender;

public interface ITelegramMessageSender
{
    Task SendAsync(string userTag, string message, ReplyMarkup? replyMarkup = null);
}