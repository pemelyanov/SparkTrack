using System.Threading.Channels;
using SparkTrack.Core.Shared.Eventing;

namespace SparkTrack.WebAPI.BackgroundHandlers;

public abstract class BackgroundEventHandlerBase<TEvent> : BackgroundService, IEventHandler<TEvent>
{
    private Channel<TEvent> m_eventsChannel = null!;
    
    public async Task HandleAsync(TEvent eventData, CancellationToken cancellationToken)
    {
        await m_eventsChannel.Writer.WriteAsync(eventData, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        m_eventsChannel = Channel.CreateUnbounded<TEvent>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingEvent = await m_eventsChannel.Reader.ReadAsync(stoppingToken);

            await HandleEventAsync(pendingEvent, stoppingToken);
        }
    }

    protected abstract Task HandleEventAsync(TEvent pendingEvent, CancellationToken cancellationToken);
}