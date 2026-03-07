namespace SparkTrack.AvaloniaImpl.Services.DeepLinkNavigation;

using DeepLink;

public interface IDeepLinkNavigationService
{
    IDisposable? Start();

    void Enqueue(SparkTrackDeepLink deepLink);
}