namespace SparkTrack.AvaloniaImpl.Controls.BonusForm;

using System.Reactive;
using System.Reactive.Disposables;
using System.Windows.Input;
using Core.Client.Services.Users;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels;

public class BonusFormViewModel : DialogViewModelBase
{
    private readonly IUsersService m_usersService;

    public BonusFormViewModel(IUsersService usersService)
    {
        m_usersService = usersService;

        LoadUsersCommand = ReactiveCommand.CreateFromTask(LoadUsersAsync);
        AcceptBonusCommand = ReactiveCommand.Create(
            () => Close(true),
            this.WhenAnyValue(
                it => it.SelectedUser,
                it => it.Payment,
                (selectedUser, payment) => selectedUser is not null && payment > 0
            )
        );
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        LoadUsersCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public IReadOnlyList<User> UsersList { get; private set; } = [];
    
    [Reactive]
    public User? SelectedUser { get; set; }
    
    [Reactive]
    public float Payment { get; set; }
    
    [Reactive]
    public string? Comment { get; set; }
    
    public ReactiveCommand<Unit, Unit> LoadUsersCommand { get; }
    
    public ICommand AcceptBonusCommand { get; }

    private async Task LoadUsersAsync()
    {
        var employees = await m_usersService.GetPageAsync(ERole.Employee, PageQuery.All);

        UsersList = employees.Items;
    }
}