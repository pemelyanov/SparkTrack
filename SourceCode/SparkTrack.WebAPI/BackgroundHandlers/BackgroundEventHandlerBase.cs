using System.Threading.Channels;
using SparkTrack.Core.Shared.Eventing;

namespace SparkTrack.WebAPI.BackgroundHandlers;

public abstract class BackgroundEventHandlerBase<TEvent> : BackgroundService, IEventHandler<TEvent>
{
    private readonly Channel<TEvent> m_eventsChannel = Channel.CreateUnbounded<TEvent>();
    
    public async Task HandleAsync(TEvent eventData, CancellationToken cancellationToken)
    {
        await m_eventsChannel.Writer.WriteAsync(eventData, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingEvent = await m_eventsChannel.Reader.ReadAsync(stoppingToken);

            await HandleEventAsync(pendingEvent, stoppingToken);
        }
    }

    protected abstract Task HandleEventAsync(TEvent pendingEvent, CancellationToken cancellationToken);
}