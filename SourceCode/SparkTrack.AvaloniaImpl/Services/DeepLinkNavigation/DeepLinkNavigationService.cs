namespace SparkTrack.AvaloniaImpl.Services.DeepLinkNavigation;

using System.Reactive.Disposables;
using System.Threading.Channels;
using Core.Shared.Data.Entities;
using Core.Shared.Services.Features;
using DeepLink;
using DeepLink.Data;
using Extensions;
using NLog;
using Pages.Feature;
using ReactiveUI;

public class DeepLinkNavigationService(
    Lazy<IScreen> hostScreen,
    IFeaturesService featuresService,
    Func<Feature, FeaturePageViewModel> featurePageFactory
) : IDeepLinkNavigationService
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    private readonly Channel<SparkTrackDeepLink> m_channel = Channel.CreateBounded<SparkTrackDeepLink>(
        new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        }
    );

    private CancellationTokenSource? m_cts;

    public IDisposable? Start()
    {
        if (m_cts is not null)
        {
            s_logger.Warn("Attemp to start service when it already started.");

            return null;
        }

        m_cts = new CancellationTokenSource();
        
        s_logger.Info("Starting service...");

        _ = Task.Run(async () =>
            {
                while (!m_cts.IsCancellationRequested)
                {
                    try
                    {
                        var deepLink = await m_channel.Reader.ReadAsync(m_cts.Token);
                        await HandleAsync(deepLink, m_cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        s_logger.Warn("Task cancelled");
                    }
                }
            }
        );

        return Disposable.Create(() =>
            {
                s_logger.Info("Stopping service...");
                m_cts?.Cancel();
                m_cts = null;
            }
        );
    }

    public void Enqueue(SparkTrackDeepLink deepLink)
    {
        s_logger.Info("Adding deeplink with data {data} to channel...", deepLink.PageData);

        var result = m_channel.Writer.TryWrite(deepLink);
        
        if(!result) s_logger.Warn("Deeplink add failed");
    }

    // TODO: Вынести обработчики под интерфейс и разные классы со своими зависимостями
    private async Task HandleAsync(SparkTrackDeepLink deepLink, CancellationToken cancellationToken)
    {
        s_logger.Info("Handling deeplink with data {data}...", deepLink.PageData);

        if (deepLink.PageData is not PageData.Feature featurePageData)
        {
            s_logger.Warn("Unsupported type if page");
            return;
        }

        try
        {
            var feature = await featuresService.GetAsync(featurePageData.Id);

            if (feature is null)
            {
                s_logger.Warn("Feature with id {id} not found", featurePageData.Id);
                return;
            }

            var viewModel = featurePageFactory(feature);

            s_logger.Info("Navigating to {page}", viewModel.GetType().Name);

            hostScreen.Value.Router.PopToOnUIThread(viewModel);
        }
        catch (Exception e)
        {
            s_logger.Warn(e);
        }
    }
}