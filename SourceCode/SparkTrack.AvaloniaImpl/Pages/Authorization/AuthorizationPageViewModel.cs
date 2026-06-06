namespace SparkTrack.AvaloniaImpl.Pages.Authorization;

using Core.Client.Enums;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using Core.Client.Data;
using Core.Client.Services.Accounts;
using Core.Client.Services.Authorization;
using Core.Client.Services.PopupNotification;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using Services.NavigationListResolver;
using Splat;

public class AuthorizationPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>             m_screen;
    private readonly IAuthorizationService     m_authorizationService;
    private readonly INavigationListResolver   m_navigationListResolver;
    private readonly IPopupNotificationService m_popupNotificationService;
    private readonly IAccountsService          m_accountsService;
    private readonly IDialogService            m_dialogService;

    public AuthorizationPageViewModel(
        Lazy<IScreen> screen,
        IAuthorizationService authorizationService,
        INavigationListResolver navigationListResolver,
        IPopupNotificationService popupNotificationService,
        IAccountsService accountsService,
        IDialogService dialogService
    )
    {
        m_screen = screen;
        m_authorizationService = authorizationService;
        m_navigationListResolver = navigationListResolver;
        m_popupNotificationService = popupNotificationService;
        m_accountsService = accountsService;
        m_dialogService = dialogService;

        AuthorizeExistingCredentialsCommand = CreateAuthorizeExistingCredentialsCommand();
        LogInCommand = CreateLogInCommand();
        UseExistingAccountCommand = ReactiveCommand.CreateFromTask<Account, Unit>(async account =>
            {
                await UseExistingAccountAsync(account);
                return Unit.Default;
            }
        );
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        AuthorizeExistingCredentialsCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        RefreshAccountsAsync().ToObservable().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "auth";

    public IScreen HostScreen => m_screen.Value;

    [Reactive]
    public string Login { get; set; } = string.Empty;

    [Reactive]
    public string Password { get; set; } = string.Empty;

    [Reactive]
    public IReadOnlyList<Account> AccountsList { get; private set; } = [];

    public ReactiveCommand<Unit, bool> AuthorizeExistingCredentialsCommand { get; }

    private ReactiveCommand<Unit, bool> CreateAuthorizeExistingCredentialsCommand() =>
        ReactiveCommand.CreateFromTask(async () =>
            {
                if (!await m_authorizationService.TryAuthorizeExistingCredentials()) return false;

                NavigateToDefaultPage();
                return true;
            }
        );

    public ReactiveCommand<Unit, Unit> LogInCommand { get; }

    private ReactiveCommand<Unit, Unit> CreateLogInCommand() => ReactiveCommand.CreateFromTask(LogInAsync);

    public ReactiveCommand<Account, Unit> UseExistingAccountCommand { get; }

    public async Task ForgetAccount(Account account)
    {
        if(!await m_dialogService.ConfirmAsync("Вы уверены что хотитие удалить аккаунт из истории?", "Удаление аккаунта из истории")) return;
        
        await m_accountsService.RemoveAccountAsync(account.Email);

        await RefreshAccountsAsync();
    }

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

    private async Task RefreshAccountsAsync()
    {
        AccountsList = await m_accountsService.GetAccountsListAsync();
    }

    private async Task UseExistingAccountAsync(Account account)
    {
        await m_accountsService.UseAccountAsync(account);

        if(await AuthorizeExistingCredentialsCommand.Execute().ToTask()) return;

        Login = account.Email;
        
        m_popupNotificationService.Show(
            ENotificationType.Warning,
            "Данные аккаунта устарели.",
            "Ошибка авторизации"
        );
    }
}