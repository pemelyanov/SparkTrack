using System.Reflection;

namespace SparkTrack.AvaloniaImpl.Windows.Main;

using System.Reactive.Disposables;
using System.Reactive.Linq;
using Controls.Account;
using Core.Client.Services.Authorization;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using NLog;
using Pages.Authorization;
using Pages.Update;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Routing;
using Services.DeepLinkNavigation;
using Services.NavigationListResolver;
using Splat;
using ILogger = NLog.ILogger;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private readonly        IDeepLinkNavigationService m_deepLinkNavigationService;
    private readonly        IAuthorizationService      m_authorizationService;
    private static readonly ILogger                    s_logger = LogManager.GetCurrentClassLogger();
    private                 IDisposable?               m_deeplinkServiceSubscription;

    public MainWindowViewModel(
        AuthorizationPageViewModel startPage,
        INavigationListResolver navigationListResolver,
        AccountViewModel accountViewModel,
        IDeepLinkNavigationService deepLinkNavigationService,
        IAuthorizationService authorizationService,
        UpdatePageViewModel? updatePage = null
    )
    {
        m_deepLinkNavigationService = deepLinkNavigationService;
        m_authorizationService = authorizationService;
        AccountViewModel = accountViewModel;
        NavigationList = navigationListResolver.NavigationList.ObserveOn(RxApp.MainThreadScheduler);

        var assembly = GetType().Assembly;
        
        Copyright = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
            .OfType<AssemblyCopyrightAttribute>()
            .FirstOrDefault()
            ?.Copyright!;

        IRoutableViewModel page = updatePage is null ? startPage : updatePage;

        Router.NavigateOnUIThread(page);
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        Router.CurrentViewModel
            .CombineLatest(NavigationList)
            .Throttle(TimeSpan.FromMicroseconds(50))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(
                args =>
                {
                    var selectedPageType = args.First.GetType();
                    var navigationList = args.Second;
                    
                    s_logger.Info("Current page changed to {page}", selectedPageType.Name);

                    if (!navigationList.Contains(selectedPageType)) return;

                    SelectedPageType = selectedPageType;
                }
            )
            .DisposeWith(disposables);

        m_authorizationService.CurrentUser
            .Select(it => it is not null)
            .DistinctUntilChanged()
            .Subscribe(isAuthorized =>
                {
                    if (isAuthorized && m_deepLinkNavigationService.Start() is { } subscription)
                    {
                        m_deeplinkServiceSubscription = subscription;
                        return;
                    }

                    ClearDeepLinkServiceSubscription();
                }
            )
            .DisposeWith(disposables);

        Disposable.Create(ClearDeepLinkServiceSubscription)
            .DisposeWith(disposables);
    }

    public RoutingState Router { get; } = CreateRouter();

    public IObservable<IReadOnlyList<Type>> NavigationList { get; }

    public AccountViewModel AccountViewModel { get; }
    
    public string Copyright { get; }

    [Reactive]
    public Type? SelectedPageType { get; set; }

    public void SelectPage(Type pageType)
    {
        var page = Locator.Current.GetService(pageType) as IRoutableViewModel;

        if (page is null) return;

        Router.PopToOnUIThread(page);
    }

    private static RoutingState CreateRouter() => new SuspendableRoutingState();
    
    private void ClearDeepLinkServiceSubscription()
    {
        m_deeplinkServiceSubscription?.Dispose();
        m_deeplinkServiceSubscription = null;
    }
}