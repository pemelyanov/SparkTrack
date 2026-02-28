namespace SparkTrack.AvaloniaImpl.Windows.UserSelection;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Core.Client.Services.Users;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels;

public class UserSelectionViewModel : DialogViewModelBase
{
    private readonly IUsersService m_usersService;

    public UserSelectionViewModel(IUsersService usersService)
    {
        m_usersService = usersService;

        LoadUsersCommand = ReactiveCommand.CreateFromTask(LoadUsersAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        this.WhenAnyValue(it => it.UserRole)
            .Select(_ => LoadUsersCommand.Execute())
            .Switch()
            .Subscribe()
            .DisposeWith(disposables);
    }

    public IReadOnlySet<Guid> ExcludedUserIdsList { get; set; } = new HashSet<Guid>(0);
    
    [Reactive]
    public ERole UserRole { get; set; }
    
    [Reactive]
    public IReadOnlyList<User> UsersList { get; private set; } = [];
    
    [Reactive]
    public User? SelectedUser { get; set; }
    
    public ReactiveCommand<Unit, Unit> LoadUsersCommand { get; }

    private async Task LoadUsersAsync()
    {
        var employees = await m_usersService.GetPageAsync(UserRole, PageQuery.All);

        if (ExcludedUserIdsList.Count == 0)
        {
            UsersList = employees.Items;
            return;
        }

        UsersList = employees.Items.Where(it => !ExcludedUserIdsList.Contains(it.Id)).ToArray();
    }
}