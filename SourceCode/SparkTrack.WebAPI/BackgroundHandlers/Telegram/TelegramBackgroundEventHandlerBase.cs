namespace SparkTrack.WebAPI.BackgroundHandlers.Telegram;

using SparkTrack.Telegram.Core.EventHandlers;

public class TelegramBackgroundEventHandler<TEvent>(ITelegramEventHandler<TEvent> eventHandler)
    : BackgroundEventHandlerBase<TEvent>
{
    protected override Task HandleEventAsync(TEvent pendingEvent, CancellationToken cancellationToken) =>
        eventHandler.HandleAsync(pendingEvent, cancellationToken);
}