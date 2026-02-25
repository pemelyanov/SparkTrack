namespace SparkTrack.Telegram.Core.EventHandlers;

using SparkTrack.Core.Shared.Eventing;

/// <summary>
/// Интерфейс-маркер для определения обработчиков событий, работающих с telegram
/// </summary>
public interface ITelegramEventHandler<in TEvent> : IEventHandler<TEvent>;