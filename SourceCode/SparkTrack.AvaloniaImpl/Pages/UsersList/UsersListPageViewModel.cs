namespace SparkTrack.AvaloniaImpl.Pages.UsersList;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ConfirmationOptions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Controls.UserAddForm;
using Controls.UserEditForm;
using Extensions;
using Services.DialogHost;
using ViewModels;
using SparkTrack.Core.Client.Events;
using SparkTrack.Core.Client.Services.Authorization;
using SparkTrack.Core.Client.Services.Users;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Eventing;
using SparkTrack.Core.Shared.Extensions;
using Reactive;

public class UsersListPageViewModel : ViewModelBase, IRoutableViewModel, IEventHandler<LogOutEvent>
{
    private readonly Lazy<IScreen>                                  m_hostScreen;
    private readonly IUsersService                                  m_usersService;
    private readonly IAuthorizationService                          m_authorizationService;
    private readonly IDialogService                                 m_dialogService;
    private readonly Func<User, UserEditFormViewModel>              m_userEditFactory;
    private readonly UserAddFormViewModel                           m_userAddFormViewModel;
    private readonly BehaviorObservableSubject<IReadOnlyList<User>> m_selectedUsers = new([]);

    public UsersListPageViewModel(
        Lazy<IScreen> hostScreen,
        UserAddFormViewModel userAddFormViewModel,
        IUsersService usersService,
        IAuthorizationService authorizationService,
        IDialogService dialogService,
        Func<User, UserEditFormViewModel> userEditFactory
    )
    {
        m_hostScreen = hostScreen;
        m_usersService = usersService;
        m_authorizationService = authorizationService;
        m_dialogService = dialogService;
        m_userEditFactory = userEditFactory;
        m_userAddFormViewModel = userAddFormViewModel;

        ReloadTableCommand = CreateReloadTableCommand();
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, m_selectedUsers.Select(it => it.Count > 0));
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        this.SetupSelectionList(it => it.CurrentPageData, m_selectedUsers);

        ReloadTableCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "users";

    public IScreen HostScreen => m_hostScreen.Value;

    [Reactive]
    public IReadOnlyList<SelectableViewModel<User>> CurrentPageData { get; private set; } = [];

    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    private ReactiveCommand<Unit, Unit> CreateReloadTableCommand() => ReactiveCommand.CreateFromTask(async () =>
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

    public async Task OpenUserEditAsync(User user)
    {
        var userEditViewModel = m_userEditFactory(user);

        if (await m_dialogService.ShowAsync(userEditViewModel) is not true) return;

        await ReloadTableCommand.Execute().ToTask();
    }

    public Task HandleAsync(LogOutEvent eventData,  CancellationToken cancellationToken = default)
    {
        m_userAddFormViewModel.Reset();

        return Task.CompletedTask;
    }

    private async Task DeleteAsync()
    {
        if (m_selectedUsers.Value.Count == 0) return;
        
        var forceOption = new ForceDeleteOption();

        if (!await m_dialogService.ConfirmAsync(
            $"Вы уверены что хотите удалить выбранных пользователей ({m_selectedUsers.Value.Count})? Пользователи имеющие связь с задачами будут добавлены в архив, остальные будут полностью удалены.",
            "Удаление пользователей",
            additionalOptionsList: [forceOption]
        )) return;

        var errorsList = new List<(Exception exception, User user)>();

        foreach (var user in m_selectedUsers.Value)
        {
            try
            {
                await m_usersService.DeleteAsync(user.Id, forceOption.IsSelected);
            }
            catch (Exception e)
            {
                errorsList.Add((e, user));
            }
        }

        if (errorsList.Count != 0)
        {
            await m_dialogService.NotifyAsync(
                $"{string.Join(";\n\n\n", errorsList.Select(it => $"{it.user.Name}: {it.exception.Message}"))}.",
                "При удалении некоторых пользователей возникли ошибки"
            );
        }

        await ReloadTableCommand.Execute().ToTask();
    }
}