namespace SparkTrack.Core.Shared.Eventing;

public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent eventData);
}