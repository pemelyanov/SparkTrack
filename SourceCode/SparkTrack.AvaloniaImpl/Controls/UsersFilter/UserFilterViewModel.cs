namespace SparkTrack.AvaloniaImpl.Controls.UsersFilter;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Core.Client.Services.Users;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class UserFilterViewModel : ViewModelBase
{
    private readonly IUsersService m_usersService;
    private          Guid?         m_idToSelectOnNextUpdate;

    public UserFilterViewModel(IUsersService usersService)
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
        
        this.WhenAnyValue(it => it.UsersList)
            .Subscribe(list =>
                {
                    if(list.Count == 0) return;
                    
                    var idToSelectOnNextUpdate = m_idToSelectOnNextUpdate;
                    m_idToSelectOnNextUpdate = null;
                    
                    if (idToSelectOnNextUpdate is null) return;

                    SelectedUser = list.FirstOrDefault(it => it.Id == idToSelectOnNextUpdate);
                }
            )
            .DisposeWith(disposables);
    }

    [Reactive]
    public ERole UserRole { get; set; }
    
    [Reactive]
    public IReadOnlyList<User> UsersList { get; private set; } = [];
    
    [Reactive]
    public User? SelectedUser { get; set; }
    
    public Guid? SelectedId => m_idToSelectOnNextUpdate ?? SelectedUser?.Id;

    [Reactive]
    public bool ShowLabel { get; set; } = true;
    
    public ReactiveCommand<Unit, Unit> LoadUsersCommand { get; }

    public void AutoSelectOnceOnNextUpdate(Guid id) => m_idToSelectOnNextUpdate = id;
    
    public IObservable<Guid?> SelectedIdChanged() => this.WhenAnyValue(it => it.SelectedUser)
        .Select(it => it?.Id)
        .StartWith(m_idToSelectOnNextUpdate)
        .DistinctUntilChanged();

    private async Task LoadUsersAsync()
    {
        var employees = await m_usersService.GetPageAsync(UserRole, PageQuery.All);

        UsersList = employees.Items;
    }
}