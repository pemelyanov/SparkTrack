namespace SparkTrack.AvaloniaImpl.Pages.Authorization;

using System.Reactive;
using System.Reactive.Disposables;
using Core.Client.Services.Authorization;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using Features;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.NavigationListResolver;
using Splat;

public class AuthorizationPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>           m_screen;
    private readonly IAuthorizationService   m_authorizationService;
    private readonly INavigationListResolver m_navigationListResolver;

    public AuthorizationPageViewModel(Lazy<IScreen> screen,
                                      IAuthorizationService authorizationService,
                                      INavigationListResolver navigationListResolver)
    {
        m_screen = screen;
        m_authorizationService = authorizationService;
        m_navigationListResolver = navigationListResolver;

        AuthorizeExistingCredentialsCommand = CreateAuthorizeExistingCredentialsCommand();
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        AuthorizeExistingCredentialsCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "auth";

    public IScreen HostScreen => m_screen.Value;

    [Reactive]
    public string Login { get; set; } = string.Empty;

    [Reactive]
    public string Password { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> AuthorizeExistingCredentialsCommand { get; }

    private ReactiveCommand<Unit, Unit> CreateAuthorizeExistingCredentialsCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            if(!await m_authorizationService.TryAuthorizeExistingCredentials()) return;

            NavigateToDefaultPage();
        }
    );

    public async Task LogInAsync()
    {
        if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password)) return;

        if (!await m_authorizationService.LogInAsync(Login, Password)) return;

        NavigateToDefaultPage();
    }

    private void NavigateToDefaultPage()
    {
        if(m_authorizationService.CurrentUser.Value?.Role is not { } role) return;

        var targetPageType = m_navigationListResolver.ResolveDefaultPageForRole(role);

        var targetPage = Locator.Current.GetService(targetPageType) as IRoutableViewModel;
        
        HostScreen.Router.NavigateOnUIThread(targetPage!);
    }
}