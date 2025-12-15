namespace SparkTrack.AvaloniaImpl.Services.NavigationListResolver;

using Reactive;

public interface INavigationListResolver
{
    IBehaviorObservable<IReadOnlyList<Type>> NavigationList { get; }
}