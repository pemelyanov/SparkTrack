namespace SparkTrack.AvaloniaImpl.Controls.UserEditForm;

using System.Reactive;
using System.Reactive.Linq;
using Core.Client.Enums;
using Core.Client.Services.Authorization;
using Core.Client.Services.PopupNotification;
using Core.Client.Services.Users;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.Clipboard;
using Services.DialogHost;
using ViewModels;

public class UserEditFormViewModel : DialogViewModelBase
{
    private readonly IUsersService             m_usersService;
    private readonly IPopupNotificationService m_popupNotificationService;
    private readonly User                      m_user;
    private readonly IDialogService            m_dialogService;
    private readonly IAuthorizationService     m_authorizationService;
    private readonly IClipboardService         m_clipboardService;

    public UserEditFormViewModel(
        IUsersService usersService,
        IPopupNotificationService popupNotificationService,
        User user,
        IDialogService dialogService,
        IAuthorizationService authorizationService,
        IClipboardService clipboardService
    )
    {
        m_usersService = usersService;
        m_popupNotificationService = popupNotificationService;
        m_user = user;
        m_dialogService = dialogService;
        m_authorizationService = authorizationService;
        m_clipboardService = clipboardService;

        Name = user.Name;
        Email = user.Email;
        TelegramTag = user.TelegramTag;

        SaveUserCommand = InitializeSaveUserCommand();
        ResetPasswordCommand = InitializeResetPasswordCommand();
    }

    [Reactive]
    public string Name { get; set; }

    [Reactive]
    public string Email { get; set; }

    [Reactive]
    public string? TelegramTag { get; set; }

    [Reactive]
    public string? GeneratedPassword { get; private set; }

    public ReactiveCommand<Unit, Unit> SaveUserCommand { get; }

    private ReactiveCommand<Unit, Unit> InitializeSaveUserCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email)) return;

            var userEdit = new UserEdit
            {
                Id = m_user.Id,
                Name = Name,
                Email = Email,
                TelegramTag = TelegramTag?.TrimStart('@')
            };

            await m_usersService.EditAsync(userEdit);

            m_popupNotificationService.Show(ENotificationType.Success, "Данные пользователя успешно обновлены.");

            Close(true);
        },
        GetIsEmailValidObservable()
            .CombineLatest(
                GetIsNameValidObservable(),
                (isEmailValid, isNameValid) => isEmailValid && isNameValid
            )
    );

    public ReactiveCommand<Unit, Unit> ResetPasswordCommand { get; }

    private ReactiveCommand<Unit, Unit> InitializeResetPasswordCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            if (!await m_dialogService.ConfirmAsync(
                "Вы уверены что хотите сбросить пароль пользователя? Будет сгенерирован новый случайный пароль.",
                "Сброс пароля"
            )) return;

            try
            {
                GeneratedPassword = await m_authorizationService.ResetPasswordAsync(m_user.Id);
                m_popupNotificationService.Show(ENotificationType.Success, "Пароль пользователя сброшен.");
            }
            catch (Exception e)
            {
                m_popupNotificationService.Show(ENotificationType.Error, $"При сбросе пароля возникли ошибки: {e.Message}");
            }
        },
        GetIsEmailValidObservable()
            .CombineLatest(
                GetIsNameValidObservable(),
                (isEmailValid, isNameValid) => isEmailValid && isNameValid
            )
    );

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
}