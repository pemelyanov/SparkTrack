namespace SparkTrack.Core.Shared.Eventing;

public interface IResultingEventHandler<in TEvent>
{
    Task<bool> HandleAsync(TEvent eventData);
}