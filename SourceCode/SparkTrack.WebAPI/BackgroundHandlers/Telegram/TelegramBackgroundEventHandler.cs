namespace SparkTrack.WebAPI.BackgroundHandlers.Telegram;

using NLog;
using SparkTrack.Telegram.Core.EventHandlers;

public class TelegramBackgroundEventHandler<TEvent>(Func<ITelegramEventHandler<TEvent>> eventHandler)
    : BackgroundEventHandlerBase<TEvent>
{
    private readonly ILogger m_logger = LogManager.GetCurrentClassLogger();
    
    protected override async Task HandleEventAsync(TEvent pendingEvent, CancellationToken cancellationToken)
    {
        try
        {
            await eventHandler().HandleAsync(pendingEvent, cancellationToken);
        }
        catch (Exception e)
        {
            m_logger.Warn(e, "Error handling event of type {type}", typeof(TEvent));
        }
    }
}