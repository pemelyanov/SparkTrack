using InlineKeyboardButton = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton;
using ParseMode = Telegram.Bot.Types.Enums.ParseMode;

namespace SparkTrack.Telegram.Core.EventHandlers;

using System.Net;
using System.Text;
using Extensions;
using NLog;
using Repositories;
using Services;
using SparkTrack.Core.Events;
using SparkTrack.Core.Services.Users;
using SparkTrack.Core.Shared.Data;
using SparkTrack.Core.Shared.Data.Entities;
using SparkTrack.Core.Shared.Enums;

public class SubTaskCompletedEventHandler(
    ITelegramMessageSender messageSender,
    ITelegramUsersRepository telegramUsersRepository,
    IUsersService usersService
) : ITelegramEventHandler<SubTaskCompletedEvent>
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    public async Task HandleAsync(
        SubTaskCompletedEvent eventData,
        CancellationToken cancellationToken = default
    )
    {
        var subTask = eventData.SubTask;
        var employee = subTask.ExecutorEmployee;

        var tasksMap = eventData.ParentFeature.TasksList.ToDictionary(it => it.Id);

        var users = eventData.ParentFeature.TasksList
            .Where(it => it.DependsOnIdList.Any(id => tasksMap[id].Id == subTask.Id))
            .Select(it => it.ExecutorEmployee)
            .Where(it => it.Id != employee.Id);

        var adminsList = eventData.ParentFeature.AuthorsList.Any()
            ? eventData.ParentFeature.AuthorsList
            : (await usersService.GetPageAsync(ERole.Admin, PageQuery.All)).Items;

        users = users.Concat(adminsList);

        foreach (var user in users.Where(it => it.TelegramTag is not null).DistinctBy(it => it.Id))
        {
            try
            {
                await TrySendAsync(eventData, cancellationToken, user, subTask);
            }
            catch (Exception e)
            {
                s_logger.Warn(e, "Message sending to {user}@{tag} failed", user.Name, user.TelegramTag);
            }
        }
    }

    private async Task TrySendAsync(
        SubTaskCompletedEvent eventData,
        CancellationToken cancellationToken,
        User user,
        SubTask subTask
    )
    {
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
            "Sending subtask completed notification to {user}@{tag} ({role})",
            user.Name,
            user.TelegramTag,
            user.Role
        );

        var message = BuildMessage(subTask, eventData.ParentFeature, telegramUser.TimeZone);

        var action = new InlineKeyboardButton("Перейти к идее", "link");

        await messageSender.SendAsync(
            telegramUser.ChatId,
            message,
            parseMode: ParseMode.Html,
            action,
            cancellationToken
        );
    }

    private string BuildMessage(SubTask task, Feature feature, TimeSpan? timeZone)
    {
        var sb = new StringBuilder();

        sb.AppendLine("✅ <b>==== Задача выполнена ====</b>");
        sb.AppendLine(
            $"{WebUtility.HtmlEncode(feature.Name)}"
        );
        sb.AppendLine();

        sb.AppendLine(
            $"<b>Канал:</b> {WebUtility.HtmlEncode(feature.Project.Name)} ({feature.Project.Link})"
        );

        var deadline = task.Deadline
            .ApplyTimeZone(timeZone)
            .ToString("dd.MM.yy HH:mm (ddd)");

        sb.AppendLine(
            $"<b>{WebUtility.HtmlEncode(task.Name)}</b> — " +
            $"Дедлайн: {deadline} {WebUtility.HtmlEncode(timeZone?.AsUtcOffset())}"
        );

        return sb.ToString();
    }
}