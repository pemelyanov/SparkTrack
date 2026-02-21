using InlineKeyboardButton = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton;
using ParseMode = Telegram.Bot.Types.Enums.ParseMode;

namespace SparkTrack.Telegram.Core.EventHandlers;

using Extensions;
using NLog;
using Repositories;
using SparkTrack.Core.Events;
using Services;

public class FeatureCreatedEventHandler(
    ITelegramMessageSender messageSender,
    ITelegramUsersRepository telegramUsersRepository
)
    : ITelegramEventHandler<FeatureCreatedEvent>
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    public async Task HandleAsync(FeatureCreatedEvent eventData, CancellationToken cancellationToken = default)
    {
        var usersToSubTasksMap = eventData.Feature.TasksList.GroupBy(it => it.ExecutorEmployee)
            .Where(it => !string.IsNullOrEmpty(it.Key.TelegramTag));

        foreach (var group in usersToSubTasksMap)
        {
            var user = group.Key;
            var tasks = group.Where(it => it.ExecutorEmployee.Id == user.Id);

            var telegramUser = await telegramUsersRepository.GetByIdAsync(user.Id);

            if (telegramUser is null)
            {
                s_logger.Warn("Cannot find telegram chat with {username} ({role}). Skipping", user.Name, user.Role);

                return;
            }

            if (!telegramUser.IsNotificationsEnabled)
            {
                s_logger.Debug("Notifications disabled for user {username} ({role})", user.Name, user.Role);
                
                return;
            }

            s_logger.Info(
                "Sending feature created info to {user}@{tag} ({role})",
                user.Name,
                user.TelegramTag,
                user.Role
            );
            
            var action = new InlineKeyboardButton("Перейти к идее", "link");

            var tasksList = tasks.Select(task =>
                $"*{task.Name}* — Дедлайн: {task.Deadline.ApplyTimeZone(telegramUser.TimeZone):dd.MM.yy HH:mm (ddd)} {telegramUser.TimeZone?.AsUtcOffset()}"
            );
            
            var message = $"  *==== Создана идея ====*\n"
                + $"{eventData.Feature.Name}\n\n"
                + $"*Канал:* {eventData.Feature.Project.Name} ({eventData.Feature.Project.Link})\n\n" +
                $"📝 *Задачи:*\n" +
                string.Join("\n", tasksList);
            
            await messageSender.SendAsync(telegramUser.ChatId, message, parseMode: ParseMode.Markdown, action, cancellationToken);
            
        }
    }
}