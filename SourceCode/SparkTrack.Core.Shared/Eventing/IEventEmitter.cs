namespace SparkTrack.Core.Shared.Eventing;

public interface IEventEmitter
{
    Task RaiseAsync<TEvent>(TEvent data);

    Task<bool> RaiseUntilFirstFailAsync<TEvent>(TEvent data);
}