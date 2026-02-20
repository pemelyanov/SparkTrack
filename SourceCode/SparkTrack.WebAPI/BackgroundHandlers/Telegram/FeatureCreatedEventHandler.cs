using JetBrains.Annotations;
using NLog;
using SparkTrack.Core.Events;
using SparkTrack.WebAPI.Services.TelegramMessageSender;
using Telegram.Bot.Types.ReplyMarkups;
using ILogger = NLog.ILogger;

namespace SparkTrack.WebAPI.BackgroundHandlers.Telegram;

[UsedImplicitly]
public class FeatureCreatedEventHandler(ITelegramMessageSender messageSender, IConfiguration configuration)
    : TelegramBackgroundEventHandlerBase<FeatureCreatedEvent>
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    protected override async Task HandleEventAsync(FeatureCreatedEvent pendingEvent,
        CancellationToken cancellationToken)
    {

        var usersToSubTasksMap = pendingEvent.Feature.TasksList.GroupBy(it => it.ExecutorEmployee)
            .Where(it => !string.IsNullOrEmpty(it.Key.TelegramTag));
        foreach (var group in usersToSubTasksMap)
        {
            var user = group.Key;
            var tasks = group.Where(it => it.ExecutorEmployee.Id == user.Id);

            s_logger.Info("Sending feature created info to {user}", user.TelegramTag);

            var tasksList = tasks.Select(task =>
                $"{task.Name}; Дедлайн: {task.Deadline:dd.MM.yy HH:mm (ddd)}; Оплата: {task.Cost:C0} (+{task.TimelyBonus:C0}).");
            
            var message = $"Создана новая идея: {pendingEvent.Feature.Name}\n" +
                          $"Задачи:\n" +
                          string.Join("\n", tasksList);

            // TODO: Сделать отдельный проект для работы с DeepLink когда буду их реализовывать
            var baseUrl = configuration.GetRequiredSection("DeepLink").GetSection("BaseUrl").Get<string>()!;
            var action = new InlineKeyboardButton("Перейти к идее", baseUrl);
            
            await messageSender.SendAsync(user.TelegramTag!, message, action);
        }
    }
}