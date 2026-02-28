namespace SparkTrack.AvaloniaImpl.Pages.Authorization;

using Core.Client.Enums;
using System.Reactive;
using System.Reactive.Disposables;
using Core.Client.Services.Authorization;
using Core.Client.Services.PopupNotification;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.NavigationListResolver;
using Splat;

public class AuthorizationPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>             m_screen;
    private readonly IAuthorizationService     m_authorizationService;
    private readonly INavigationListResolver   m_navigationListResolver;
    private readonly IPopupNotificationService m_popupNotificationService;

    public AuthorizationPageViewModel(Lazy<IScreen> screen,
                                      IAuthorizationService authorizationService,
                                      INavigationListResolver navigationListResolver, 
                                      IPopupNotificationService popupNotificationService)
    {
        m_screen = screen;
        m_authorizationService = authorizationService;
        m_navigationListResolver = navigationListResolver;
        m_popupNotificationService = popupNotificationService;

        AuthorizeExistingCredentialsCommand = CreateAuthorizeExistingCredentialsCommand();
        LogInCommand = CreateLogInCommand();
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
    
    public ReactiveCommand<Unit, Unit> LogInCommand { get; }

    private ReactiveCommand<Unit, Unit> CreateLogInCommand() => ReactiveCommand.CreateFromTask(LogInAsync);

    private async Task LogInAsync()
    {
        if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
        {
            m_popupNotificationService.Show(ENotificationType.Warning, "Введите логин и пароль", "Ошибка авторизации");
            return;
        }

        if (!await m_authorizationService.LogInAsync(Login, Password))
        {
            m_popupNotificationService.Show(ENotificationType.Error, "Неверный логин или пароль", "Ошибка авторизации");
            return;
        }

        NavigateToDefaultPage();

        Login = string.Empty;
        Password = string.Empty;
    }

    private void NavigateToDefaultPage()
    {
        if(m_authorizationService.CurrentUser.Value?.Role is not { } role) return;

        var targetPageType = m_navigationListResolver.ResolveDefaultPageForRole(role);

        var targetPage = Locator.Current.GetService(targetPageType) as IRoutableViewModel;
        
        HostScreen.Router.ResetToOnUIThread(targetPage!);
    }
}