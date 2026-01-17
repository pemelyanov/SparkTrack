namespace SparkTrack.Core.Shared.Extensions;

using Eventing;

public static class EventEmitterExtensions
{
    public static Task RaiseAsync<TEvent>(this IEventEmitter emitter) where TEvent : new() =>
        emitter.RaiseAsync(new TEvent());
    
    public static Task<bool> RaiseUntilFirstFailAsync<TEvent>(this IEventEmitter emitter) where TEvent : new() =>
        emitter.RaiseUntilFirstFailAsync(new TEvent());
}