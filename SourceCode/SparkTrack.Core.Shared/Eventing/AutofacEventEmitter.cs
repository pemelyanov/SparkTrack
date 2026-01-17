namespace SparkTrack.Core.Shared.Eventing;

using Autofac;

public class AutofacEventEmitter(ILifetimeScope lifetimeScope) : IEventEmitter
{
    public async Task RaiseAsync<TEvent>(TEvent data)
    {
        var handlers = lifetimeScope.Resolve<IEnumerable<IEventHandler<TEvent>>>();

        foreach (var handler in handlers)
            await handler.HandleAsync(data);
    }

    public async Task<bool> RaiseUntilFirstFailAsync<TEvent>(TEvent data)
    {
        var handlers = lifetimeScope.Resolve<IEnumerable<IResultingEventHandler<TEvent>>>();

        foreach (var handler in handlers)
            if (!await handler.HandleAsync(data))
                return false;

        return true;
    }
}