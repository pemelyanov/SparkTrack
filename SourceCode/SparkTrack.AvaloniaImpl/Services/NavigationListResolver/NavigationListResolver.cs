namespace SparkTrack.AvaloniaImpl.Services.NavigationListResolver;

using System.Reactive.Linq;
using Core.Client.Services.Authorization;
using Core.Shared.Enums;
using Pages.Features;
using Reactive;

public class NavigationListResolver : INavigationListResolver, IDisposable
{
    private readonly BehaviorObservableSubject<IReadOnlyList<Type>> m_navigationList = new([]);

    private readonly Dictionary<ERole, IReadOnlyList<Type>> m_navigationListByRoleMap = new()
    {
        [ERole.Admin] = [typeof(FeaturesPageViewModel)],
        [ERole.Employee] = [typeof(FeaturesPageViewModel)],
        [ERole.God] = [typeof(FeaturesPageViewModel)]
    };

    private readonly IDisposable m_authorizationSubscription;

    public NavigationListResolver(IAuthorizationService authorizationService)
    {
        
        m_authorizationSubscription = authorizationService.CurrentUser
            .Select(user => user?.Role is null ? [] : m_navigationListByRoleMap[user.Role])
            .Do(
                it =>
                {
                    
                })
            .Subscribe(m_navigationList);
    }

    public void Dispose()
    {
        m_authorizationSubscription.Dispose();
    }

    public IBehaviorObservable<IReadOnlyList<Type>> NavigationList => m_navigationList;

    public Type ResolveDefaultPageForRole(ERole role) => m_navigationListByRoleMap[role][0];
}