namespace SparkTrack.AvaloniaImpl.Services.NavigationListResolver;

using Core.Shared.Enums;
using Reactive;

public interface INavigationListResolver
{
    IBehaviorObservable<IReadOnlyList<Type>> NavigationList { get; }

    Type ResolveDefaultPageForRole(ERole role);
}