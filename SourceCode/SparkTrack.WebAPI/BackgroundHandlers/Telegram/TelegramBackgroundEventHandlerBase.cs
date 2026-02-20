namespace SparkTrack.WebAPI.BackgroundHandlers.Telegram;

public abstract class TelegramBackgroundEventHandlerBase<TEvent> : BackgroundEventHandlerBase<TEvent>,
    ITelegramEventHandler<TEvent>;