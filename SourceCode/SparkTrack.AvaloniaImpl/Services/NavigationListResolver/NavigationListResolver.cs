using SparkTrack.AvaloniaImpl.Pages.Settings;

namespace SparkTrack.AvaloniaImpl.Services.NavigationListResolver;

using System.Reactive.Linq;
using Core.Client.Services.Authorization;
using Core.Shared.Enums;
using Pages.AdminFinance;
using Pages.FeaturesList;
using Pages.ProjectsList;
using Pages.UsersList;
using Reactive;

public class NavigationListResolver : INavigationListResolver, IDisposable
{
    private readonly BehaviorObservableSubject<IReadOnlyList<Type>> m_navigationList = new([]);

    private readonly Dictionary<ERole, IReadOnlyList<Type>> m_navigationListByRoleMap = new()
    {
        [ERole.Admin] = [typeof(FeaturesListPageViewModel), typeof(UsersListPageViewModel), typeof(AdminFinancePageViewModel), typeof(SettingsPageViewModel)],
        [ERole.Employee] = [typeof(FeaturesListPageViewModel), typeof(SettingsPageViewModel)],
        [ERole.God] = [typeof(UsersListPageViewModel), typeof(ProjectsListPageViewModel), typeof(SettingsPageViewModel)]
    };

    private readonly IDisposable m_authorizationSubscription;

    public NavigationListResolver(IAuthorizationService authorizationService)
    {
        
        m_authorizationSubscription = authorizationService.CurrentUser
            .Select(user => user?.Role is null ? [] : m_navigationListByRoleMap[user.Role])
            .Subscribe(m_navigationList);
    }

    public void Dispose()
    {
        m_authorizationSubscription.Dispose();
    }

    public IBehaviorObservable<IReadOnlyList<Type>> NavigationList => m_navigationList;

    public Type ResolveDefaultPageForRole(ERole role) => m_navigationListByRoleMap[role][0];
}