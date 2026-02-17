namespace SparkTrack.AvaloniaImpl.Controls.UserAddForm;

using System.Reactive;
using System.Reactive.Linq;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkTrack.Core.Client.Services.Authorization;
using SparkTrack.Core.Client.Services.PopupNotification;
using SparkTrack.Core.Client.Services.Users;
using Core.Shared.Data.Edit;
using Services.Clipboard;
using SparkTrack.Core.Shared.Enums;
using SparkTrack.Core.Shared.Extensions;

public class UserAddFormViewModel : ViewModelBase
{
    private readonly IUsersService             m_usersService;
    private readonly IPopupNotificationService m_popupNotificationService;
    private readonly IClipboardService         m_clipboardService;
    private readonly IAuthorizationService     m_authorizationService;

    public UserAddFormViewModel(
        IAuthorizationService authorizationService,
        IUsersService usersService,
        IPopupNotificationService popupNotificationService,
        IClipboardService clipboardService
    )
    {
        m_usersService = usersService;
        m_popupNotificationService = popupNotificationService;
        m_clipboardService = clipboardService;
        m_authorizationService = authorizationService;

        SaveUserCommand = InitializeSaveUserCommand();
    }

    [Reactive]
    public string Name { get; set; } = string.Empty;

    [Reactive]
    public string Email { get; set; } = string.Empty;
    
    [Reactive]
    public string? TelegramTag { get; set; }

    [Reactive]
    public string? GeneratedPassword { get; private set; }

    public ReactiveCommand<Unit, Unit> SaveUserCommand { get; }

    private ReactiveCommand<Unit, Unit> InitializeSaveUserCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email)
                || m_authorizationService.CurrentUser.Value?.Role is not { } currentUserRole) return;

            if (currentUserRole is ERole.Employee) return;

            var createdUserRole = currentUserRole.ResolveSubordinateRole();

            var userEdit = new UserEdit
            {
                Name = Name,
                Email = Email,
                TelegramTag = TelegramTag?.TrimStart('@')
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
    
    public void Reset()
    {
        Email = string.Empty;
        Name = string.Empty;
        TelegramTag = null;
        GeneratedPassword = string.Empty;
    }

    public async Task CopyGeneratedPasswordAsync()
    {
        if(GeneratedPassword is null) return;

        await m_clipboardService.SaveToClipboardAsync(GeneratedPassword, "Пароль скопирован в буфер обмена");
    }

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
}