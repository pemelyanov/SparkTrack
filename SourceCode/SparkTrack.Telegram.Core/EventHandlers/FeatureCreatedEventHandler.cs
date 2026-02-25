using InlineKeyboardButton = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton;
using ParseMode = Telegram.Bot.Types.Enums.ParseMode;

namespace SparkTrack.Telegram.Core.EventHandlers;

using NLog;
using Repositories;
using SparkTrack.Core.Events;
using Services;
using System.Net;
using System.Text;
using Data;
using Extensions;
using SparkTrack.Core.Shared.Data.Entities;

public class FeatureCreatedEventHandler(
    ITelegramMessageSender messageSender,
    ITelegramUsersRepository telegramUsersRepository
)
    : ITelegramEventHandler<FeatureCreatedEvent>
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    public async Task HandleAsync(
        FeatureCreatedEvent eventData,
        CancellationToken cancellationToken = default
    )
    {
        var usersToSubTasksMap = eventData.Feature.TasksList
            .GroupBy(it => it.ExecutorEmployee)
            .Where(it => !string.IsNullOrEmpty(it.Key.TelegramTag));

        foreach (var group in usersToSubTasksMap)
        {
            try
            {
                await TrySendAsync(eventData, cancellationToken, group);
            }
            catch (Exception e)
            {
                s_logger.Warn(e, "Message sending to {user}@{tag} failed", group.Key.Name, group.Key.TelegramTag);
            }
        }
    }

    private async Task TrySendAsync(
        FeatureCreatedEvent eventData,
        CancellationToken cancellationToken,
        IGrouping<User, SubTask> group
    )
    {
        var user = group.Key;
        var tasks = group.Where(it => it.ExecutorEmployee.Id == user.Id);

        var telegramUser = await telegramUsersRepository.GetByTagAsync(user.TelegramTag!);

        if (telegramUser is null)
        {
            s_logger.Warn(
                "Cannot find telegram chat with {username} ({role}). Skipping",
                user.Name,
                user.Role
            );
            return;
        }

        if (!telegramUser.IsNotificationsEnabled)
        {
            s_logger.Debug(
                "Notifications disabled for user {username} ({role})",
                user.Name,
                user.Role
            );
            return;
        }

        s_logger.Info(
            "Sending feature created info to {user}@{tag} ({role})",
            user.Name,
            user.TelegramTag,
            user.Role
        );

        var action = new InlineKeyboardButton("Перейти к идее", "link");

        var message = BuildMessage(eventData.Feature, tasks, telegramUser);

        await messageSender.SendAsync(
            telegramUser.ChatId,
            message,
            parseMode: ParseMode.Html,
            action,
            cancellationToken
        );
    }

    private string BuildMessage(
        Feature feature,
        IEnumerable<SubTask> tasks,
        TelegramUser telegramUser
    )
    {
        var sb = new StringBuilder();

        sb.AppendLine("<b>==== Создана идея ====</b>");
        sb.AppendLine(WebUtility.HtmlEncode(feature.Name));
        sb.AppendLine();

        sb.AppendLine(
            $"<b>Канал:</b> {WebUtility.HtmlEncode(feature.Project.Name)} " +
            $"({feature.Project.Link})"
        );
        sb.AppendLine();

        sb.AppendLine("📝 <b>Задачи:</b>");

        foreach (var task in tasks)
        {
            var deadline = task.Deadline
                .ApplyTimeZone(telegramUser.TimeZone)
                .ToString("dd.MM.yy HH:mm (ddd)");

            var timeZone = telegramUser.TimeZone?.AsUtcOffset();

            sb.AppendLine(
                $"<b>{WebUtility.HtmlEncode(task.Name)}</b> — " +
                $"Дедлайн: {deadline} {WebUtility.HtmlEncode(timeZone)}"
            );
        }

        return sb.ToString();
    }
}