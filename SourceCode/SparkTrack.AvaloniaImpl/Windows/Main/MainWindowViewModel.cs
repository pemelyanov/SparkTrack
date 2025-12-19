namespace SparkTrack.AvaloniaImpl.Windows.Main;

using System.Reactive.Disposables;
using Controls.Account;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using Pages.Authorization;
using Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.NavigationListResolver;
using Splat;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private readonly INavigationListResolver m_navigationListResolver;

    public MainWindowViewModel(
        AuthorizationPageViewModel startPage,
        INavigationListResolver navigationListResolver,
        AccountViewModel accountViewModel
    )
    {
        m_navigationListResolver = navigationListResolver;
        AccountViewModel = accountViewModel;
        Router.NavigateOnUIThread(startPage);
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        Router.CurrentViewModel.Subscribe(it => SelectedPageType = it.GetType()).DisposeWith(disposables);
    }

    public RoutingState Router { get; } = new();

    public IBehaviorObservable<IReadOnlyList<Type>> NavigationList => m_navigationListResolver.NavigationList;

    public AccountViewModel AccountViewModel { get; }

    [Reactive]
    public Type? SelectedPageType { get; set; }

    public void SelectPage(Type pageType)
    {
        var page = Locator.Current.GetService(pageType) as IRoutableViewModel;

        if (page is null) return;

        Router.PopToOnUIThread(page);
    }
}