namespace SparkTrack.AvaloniaImpl.Pages.Users;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using Controls.UserAddForm;
using Core.Client.Events;
using Core.Client.Services.Authorization;
using Core.Client.Services.Users;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Eventing;
using Core.Shared.Extensions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using ViewModels;

public class UsersPageViewModel : ViewModelBase, IRoutableViewModel, IEventHandler<LogoutEvent>
{
    private readonly Lazy<IScreen>               m_hostScreen;
    private readonly IUsersService               m_usersService;
    private readonly IAuthorizationService       m_authorizationService;
    private readonly IDialogService                 m_dialogService;
    private readonly Func<UserAddFormViewModel> m_userEditFactory;
    private readonly UserAddFormViewModel       m_userAddFormViewModel;

    public UsersPageViewModel(Lazy<IScreen> hostScreen,
                              UserAddFormViewModel userAddFormViewModel,
                              IUsersService usersService,
                              IAuthorizationService authorizationService,
                              IDialogService dialogService,
                              Func<UserAddFormViewModel> userEditFactory)
    {
        m_hostScreen = hostScreen;
        m_usersService = usersService;
        m_authorizationService = authorizationService;
        m_dialogService = dialogService;
        m_userEditFactory = userEditFactory;
        m_userAddFormViewModel = userAddFormViewModel;

        ReloadTableCommand = CreateReloadTableCommand();
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ReloadTableCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "users";

    public IScreen HostScreen => m_hostScreen.Value;

    [Reactive]
    public IReadOnlyList<SelectableViewModel<User>> CurrentPageData { get; private set; } = [];

    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    private ReactiveCommand<Unit, Unit> CreateReloadTableCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            if (m_authorizationService.CurrentUser.Value?.Role is not { } currentUserRole) return;

            var page = await m_usersService.GetPageAsync(currentUserRole.ResolveSubordinateRole(), PageQuery.All);

            CurrentPageData = page.Items.Select(it => new SelectableViewModel<User>(it)).ToArray();
        }
    );

    public async Task OpenUserAddAsync()
    {
        await m_dialogService.ShowAsync(m_userAddFormViewModel);

        await ReloadTableCommand.Execute().ToTask();
    }

    public Task HandleAsync(LogoutEvent eventData)
    {
        m_userAddFormViewModel.Reset();
        
        return Task.CompletedTask;
    }
}