namespace SparkTrack.AvaloniaImpl.Controls.UserEditForm;

using System.Reactive;
using System.Reactive.Linq;
using Core.Client.Services.Authorization;
using Core.Client.Services.Users;
using Core.Shared.Data.Edit;
using Core.Shared.Enums;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;

public class UserEditFormViewModel : ViewModelBase
{
    private readonly IUsersService         m_usersService;
    private readonly IAuthorizationService m_authorizationService;

    public UserEditFormViewModel(IUsersService usersService, IAuthorizationService authorizationService)
    {
        m_usersService = usersService;
        m_authorizationService = authorizationService;

        CreateUserCommand = InitializeCreateUserCommand();
    }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? GeneratedPassword { get; private set; }

    public ReactiveCommand<Unit, Unit> CreateUserCommand { get; }

    private ReactiveCommand<Unit, Unit> InitializeCreateUserCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email)
                || m_authorizationService.CurrentUser.Value?.Role is not { } currentUserRole) return;

            if (currentUserRole is ERole.Employee) return;

            var createdUserRole = ResolveSubordinateRole(currentUserRole);

            var userEdit = new UserEdit
            {
                Name = Name,
                Email = Email
            };

            GeneratedPassword = await m_usersService.AddAsync(userEdit, createdUserRole);
        },
        GetIsEmailValidObservable()
            .CombineLatest(
                GetIsNameValidObservable(),
                GetIsPasswordGeneratedObservable(),
                (isEmailValid, isNameValid, isPasswordGenerated) => isEmailValid && isNameValid && !isPasswordGenerated
            )
    );

    private IObservable<bool> GetIsEmailValidObservable()
    {
        return this.WhenAnyValue(it => it.Email)
            .Select(it => !string.IsNullOrEmpty(it));
    }

    private IObservable<bool> GetIsNameValidObservable()
    {
        return this.WhenAnyValue(it => it.Name)
            .Select(it => !string.IsNullOrEmpty(it));
    }

    private IObservable<bool> GetIsPasswordGeneratedObservable()
    {
        return this.WhenAnyValue(it => it.GeneratedPassword)
            .Select(it => !string.IsNullOrEmpty(it));
    }

    private static ERole ResolveSubordinateRole(ERole currentUserRole) => currentUserRole switch
    {
        ERole.Admin => ERole.Employee,
        ERole.God => ERole.Admin,
        _ => throw new NotSupportedException()
    };
}