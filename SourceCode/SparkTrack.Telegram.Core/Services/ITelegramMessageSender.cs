using ReplyMarkup = Telegram.Bot.Types.ReplyMarkups.ReplyMarkup;

namespace SparkTrack.Telegram.Core.Services;

public interface ITelegramMessageSender
{
    Task SendAsync(
        long chatId,
        string message,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default
    );
}