using ParseMode = Telegram.Bot.Types.Enums.ParseMode;

namespace SparkTrack.Telegram.Core.EventHandlers;

using System.Net;
using System.Text;
using NLog;
using Repositories;
using Services;
using SparkTrack.Core.Events;
using SparkTrack.Core.Shared.Data.Entities;
using SparkTrack.Core.Shared.Enums;

public class FeatureDeletedEventHandler(
    ITelegramMessageSender messageSender,
    ITelegramUsersRepository telegramUsersRepository
) : ITelegramEventHandler<FeatureDeletedEvent>
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    public async Task HandleAsync(FeatureDeletedEvent eventData, CancellationToken cancellationToken = default)
    {
        var feature = eventData.Feature;

        var users = feature.TasksList.Select(t => t.ExecutorEmployee)
            .Where(e => !string.IsNullOrEmpty(e.TelegramTag))
            .DistinctBy(e => e.Id);

        foreach (var user in users.Where(it => it.TelegramTag is not null))
        {
            try
            {
                await TrySendAsync(eventData, cancellationToken, user, feature);
            }
            catch (Exception e)
            {
                s_logger.Warn(e, "Message sending to {user}@{tag} failed", user.Name, user.TelegramTag);
            }
        }
    }

    private async Task TrySendAsync(
        FeatureDeletedEvent eventData,
        CancellationToken cancellationToken,
        User user,
        Feature feature
    )
    {
        var telegramUser = await telegramUsersRepository.GetByTagAsync(user.TelegramTag!);

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

        s_logger.Info("Sending feature deleted info to {user}@{tag} ({role})", user.Name, user.TelegramTag, user.Role);

        var message = BuildMessage(feature, user, eventData.Reason);

        await messageSender.SendAsync(
            telegramUser.ChatId,
            message,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken
        );
    }

    private string BuildMessage(Feature feature, User employee, EArchiveSource? reason)
    {
        var isArchived = reason.HasValue;

        var header = isArchived ? "<b>==== Идея отправлена в архив ====</b>" : "<b>==== Идея удалена ====</b>";

        var message = new StringBuilder();

        message.AppendLine(header);
        message.AppendLine(WebUtility.HtmlEncode(feature.Name));
        message.AppendLine();

        message.AppendLine(
            $"<b>Канал:</b> {WebUtility.HtmlEncode(feature.Project.Name)} " + $"({feature.Project.Link})"
        );
        message.AppendLine();

        if (isArchived)
        {
            message.AppendLine($"<b>Причина:</b> {WebUtility.HtmlEncode(GetArchiveReasonText(reason!.Value))}");
            message.AppendLine();
        }

        var userTasks = feature.TasksList.Where(t => t.ExecutorEmployee.Id == employee.Id).ToList();

        if (userTasks.Any())
        {
            message.AppendLine("📝 <b>Задачи в идее:</b>");

            foreach (var task in userTasks)
            {
                message.AppendLine(WebUtility.HtmlEncode(task.Name));
            }

            message.AppendLine();
        }

        return message.ToString();
    }

    private string GetArchiveReasonText(EArchiveSource reason)
    {
        return reason switch
        {
            EArchiveSource.User => "Идея была отправлена в архив администратором",
            EArchiveSource.Parent => "Родительский канад был заархивирован",
            _ => "Не указана"
        };
    }
}