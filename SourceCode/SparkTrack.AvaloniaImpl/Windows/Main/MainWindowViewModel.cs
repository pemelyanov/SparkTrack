namespace SparkTrack.AvaloniaImpl.Windows.Main;

using System.Reactive.Disposables;
using System.Reactive.Linq;
using Controls.Account;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using NLog;
using Pages.Authorization;
using Pages.Update;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Routing;
using Services.NavigationListResolver;
using Splat;
using ILogger = NLog.ILogger;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    public MainWindowViewModel(
        AuthorizationPageViewModel startPage,
        INavigationListResolver navigationListResolver,
        AccountViewModel accountViewModel,
        UpdatePageViewModel? updatePage = null
    )
    {
        AccountViewModel = accountViewModel;
        NavigationList = navigationListResolver.NavigationList.ObserveOn(RxApp.MainThreadScheduler);

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

                    if (!navigationList.Contains(selectedPageType)) return;

                    SelectedPageType = selectedPageType;

                    s_logger.Info("Selected page changed to {page}", SelectedPageType.Name);
                }
            )
            .DisposeWith(disposables);
    }

    public RoutingState Router { get; } = CreateRouter();

    public IObservable<IReadOnlyList<Type>> NavigationList { get; }

    public AccountViewModel AccountViewModel { get; }

    [Reactive]
    public Type? SelectedPageType { get; set; }

    public void SelectPage(Type pageType)
    {
        var page = Locator.Current.GetService(pageType) as IRoutableViewModel;

        if (page is null) return;

        Router.PopToOnUIThread(page);
    }

    private static RoutingState CreateRouter() => new SuspendableRoutingState();
}