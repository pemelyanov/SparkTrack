using SparkTrack.Core.Shared.Eventing;

namespace SparkTrack.WebAPI.BackgroundHandlers.Telegram;

public interface ITelegramEventHandler<in TEvent> : IHostedService, IEventHandler<TEvent>;