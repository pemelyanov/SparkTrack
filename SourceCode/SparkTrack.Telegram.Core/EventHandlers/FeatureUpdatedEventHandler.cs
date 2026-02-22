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
using SparkTrack.Core.Shared.Data.Entities;

public class FeatureUpdatedEventHandler(
    ITelegramMessageSender messageSender,
    ITelegramUsersRepository telegramUsersRepository
) : ITelegramEventHandler<FeatureUpdatedEvent>
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    public async Task HandleAsync(FeatureUpdatedEvent eventData, CancellationToken cancellationToken = default)
    {
        var oldFeature = eventData.OldInfo;
        var newFeature = eventData.NewInfo;

        // Получаем всех сотрудников, у которых были изменения в задачах
        var affectedEmployees = GetAffectedEmployees(oldFeature, newFeature);

        foreach (var employee in affectedEmployees.Where(it => it.TelegramTag is not null))
        {
            var telegramUser = await telegramUsersRepository.GetByTagAsync(employee.TelegramTag!);

            if (telegramUser is null)
            {
                s_logger.Warn(
                    "Cannot find telegram chat with {username} ({role}). Skipping",
                    employee.Name,
                    employee.Role
                );
                continue;
            }

            if (!telegramUser.IsNotificationsEnabled)
            {
                s_logger.Debug(
                    "Notifications disabled for user {username} ({role})",
                    employee.Name,
                    employee.Role
                );
                continue;
            }

            s_logger.Info(
                "Sending feature updated info to {user}@{tag} ({role})",
                employee.Name,
                employee.TelegramTag,
                employee.Role
            );

            var changes = GetChangesForEmployee(oldFeature, newFeature, employee.Id, telegramUser.TimeZone);
            var message = BuildMessage(newFeature, changes, telegramUser.TimeZone);

            var action = new InlineKeyboardButton("Перейти к идее", "link");

            await messageSender.SendAsync(
                telegramUser.ChatId,
                message,
                parseMode: ParseMode.Html,
                action,
                cancellationToken
            );
        }
    }

    private IEnumerable<User> GetAffectedEmployees(Feature oldFeature, Feature newFeature)
    {
        var allEmployees = newFeature.TasksList
            .Select(t => t.ExecutorEmployee)
            .Concat(oldFeature.TasksList.Select(t => t.ExecutorEmployee))
            .Where(e => !string.IsNullOrEmpty(e.TelegramTag))
            .DistinctBy(e => e.Id);

        return allEmployees.Where(employee =>
            HasChangesForEmployee(oldFeature, newFeature, employee.Id)
        );
    }

    private bool HasChangesForEmployee(Feature oldFeature, Feature newFeature, Guid employeeId)
    {
        var oldEmployeeTasks = oldFeature.TasksList
            .Where(t => t.ExecutorEmployee.Id == employeeId)
            .ToList();

        var newEmployeeTasks = newFeature.TasksList
            .Where(t => t.ExecutorEmployee.Id == employeeId)
            .ToList();

        if (oldEmployeeTasks.Count != newEmployeeTasks.Count)
            return true;

        var oldTasksDict = oldEmployeeTasks.ToDictionary(t => t.Id);
        var newTasksDict = newEmployeeTasks.ToDictionary(t => t.Id);

        foreach (var newTask in newEmployeeTasks)
        {
            if (!oldTasksDict.TryGetValue(newTask.Id, out var oldTask))
                return true;

            if (HasTaskChanges(oldTask, newTask))
                return true;
        }

        foreach (var oldTask in oldEmployeeTasks)
        {
            if (!newTasksDict.ContainsKey(oldTask.Id))
                return true;
        }

        return false;
    }

    private bool HasTaskChanges(SubTask oldTask, SubTask newTask)
    {
        return oldTask.Name != newTask.Name || oldTask.Deadline != newTask.Deadline
            || oldTask.ExecutorEmployee.Id != newTask.ExecutorEmployee.Id;
    }

    private List<TaskChange> GetChangesForEmployee(
        Feature oldFeature,
        Feature newFeature,
        Guid employeeId,
        TimeSpan? timeZone
    )
    {
        var changes = new List<TaskChange>();

        var oldTasks = oldFeature.TasksList
            .Where(t => t.ExecutorEmployee.Id == employeeId)
            .ToDictionary(t => t.Id);

        var newTasks = newFeature.TasksList
            .Where(t => t.ExecutorEmployee.Id == employeeId)
            .ToDictionary(t => t.Id);

        foreach (var newTask in newTasks.Values)
        {
            if (!oldTasks.ContainsKey(newTask.Id))
            {
                changes.Add(TaskChange.Added(newTask));
            }
        }

        foreach (var oldTask in oldTasks.Values)
        {
            if (!newTasks.ContainsKey(oldTask.Id))
            {
                changes.Add(TaskChange.Removed(oldTask));
            }
        }

        foreach (var newTask in newTasks.Values)
        {
            if (oldTasks.TryGetValue(newTask.Id, out var oldTask))
            {
                var taskModifications = GetTaskModifications(oldTask, newTask, timeZone);
                if (taskModifications.Any())
                {
                    changes.Add(TaskChange.Modified(newTask, taskModifications));
                }
            }
        }

        return changes;
    }

    private List<TaskModification> GetTaskModifications(SubTask oldTask, SubTask newTask, TimeSpan? timeZone)
    {
        var modifications = new List<TaskModification>();

        if (oldTask.Name != newTask.Name)
            modifications.Add(new TaskModification("Название", oldTask.Name, newTask.Name));

        if (oldTask.Deadline != newTask.Deadline)
            modifications.Add(
                new TaskModification(
                    "Дедлайн",
                    oldTask.Deadline.ApplyTimeZone(timeZone).ToString("dd.MM.yy HH:mm"),
                    newTask.Deadline.ApplyTimeZone(timeZone).ToString("dd.MM.yy HH:mm")
                )
            );

        return modifications;
    }

    private string BuildMessage(
        Feature feature,
        List<TaskChange> changes,
        TimeSpan? timeZone
    )
    {
        var sb = new StringBuilder();

        sb.AppendLine("<b>==== Идея обновлена ====</b>");
        sb.AppendLine(WebUtility.HtmlEncode(feature.Name));
        sb.AppendLine();

        sb.AppendLine(
            $"<b>Канал:</b> {WebUtility.HtmlEncode(feature.Project.Name)} " +
            $"({feature.Project.Link})"
        );
        sb.AppendLine();

        if (!changes.Any())
        {
            sb.AppendLine("❓ <b>Изменений в задачах не обнаружено</b>");
            return sb.ToString();
        }

        sb.AppendLine("📝 <b>Изменения в задачах:</b>");

        foreach (var change in changes.OrderBy(c => c.Type))
        {
            switch (change.Type)
            {
                case ChangeType.Added:
                    sb.AppendLine(
                        $"✅ <b>Новая задача:</b> {FormatTask(change.Task, timeZone)}"
                    );
                    break;

                case ChangeType.Removed:
                    sb.AppendLine(
                        $"❌ <b>Удалена задача:</b> {FormatTask(change.Task, timeZone)}"
                    );
                    break;

                case ChangeType.Modified:
                    sb.AppendLine(FormatModifiedTask(change, timeZone));
                    break;
            }
        }

        return sb.ToString();
    }

    private string FormatTask(SubTask task, TimeSpan? timeZone)
    {
        var deadline =
            $" — Дедлайн: {task.Deadline.ApplyTimeZone(timeZone):dd.MM.yy HH:mm (ddd)} " +
            $"{timeZone?.AsUtcOffset()}";

        return
            $"<b>{WebUtility.HtmlEncode(task.Name)}</b>" +
            WebUtility.HtmlEncode(deadline);
    }

    private string FormatModifiedTask(TaskChange change, TimeSpan? timeZone)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            $"✏️ <b>Изменена задача:</b> {FormatTask(change.Task, timeZone)}"
        );

        foreach (var modification in change.Modifications)
        {
            sb.AppendLine(
                $"  • {WebUtility.HtmlEncode(modification.Field)}: " +
                $"<b>{WebUtility.HtmlEncode(modification.OldValue)}</b> → " +
                $"<b>{WebUtility.HtmlEncode(modification.NewValue)}</b>"
            );
        }

        return sb.ToString();
    }

    private record TaskChange(SubTask Task, ChangeType Type, List<TaskModification> Modifications)
    {
        public static TaskChange Added(SubTask task) => new(task, ChangeType.Added, []);

        public static TaskChange Removed(SubTask task) => new(task, ChangeType.Removed, []);

        public static TaskChange Modified(SubTask task, List<TaskModification> modifications) =>
            new(task, ChangeType.Modified, modifications);
    }

    private record TaskModification(string Field, string OldValue, string NewValue);

    private enum ChangeType
    {
        Added,
        Removed,
        Modified
    }
}