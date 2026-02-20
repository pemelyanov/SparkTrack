using NLog;
using SparkTrack.Core.Events;
using SparkTrack.WebAPI.Services.TelegramMessageSender;
using ILogger = NLog.ILogger;

namespace SparkTrack.WebAPI.BackgroundHandlers.Telegram;

public class FeatureCreatedEventHandler(ITelegramMessageSender messageSender)
    : TelegramBackgroundEventHandlerBase<FeatureCreatedEvent>
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    protected override async Task HandleEventAsync(FeatureCreatedEvent pendingEvent,
        CancellationToken cancellationToken)
    {
        foreach (var subTask in pendingEvent.Feature.TasksList)
        {
            if (subTask.ExecutorEmployee.TelegramTag is null) continue;

            s_logger.Info("Sending feature created info to {user}", subTask.ExecutorEmployee.TelegramTag);
            
            await messageSender.SendAsync(subTask.ExecutorEmployee.TelegramTag, $"Новая задача: {subTask.Name}");
        }
    }
}