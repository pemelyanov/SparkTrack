using ParseMode = Telegram.Bot.Types.Enums.ParseMode;
using ReplyMarkup = Telegram.Bot.Types.ReplyMarkups.ReplyMarkup;

namespace SparkTrack.Telegram.Core.Services;

public interface ITelegramMessageSender
{
    Task SendAsync(
        long chatId,
        string message,
        ParseMode parseMode = ParseMode.None,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default
    );
}