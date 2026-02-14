namespace SparkTrack.AvaloniaImpl.Controls.UserEditForm;

using System.Reactive;
using System.Reactive.Linq;
using Core.Client.Enums;
using Core.Client.Services.PopupNotification;
using Core.Client.Services.Users;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels;

public class UserEditFormViewModel : DialogViewModelBase
{
    private readonly IUsersService             m_usersService;
    private readonly IPopupNotificationService m_popupNotificationService;
    private readonly User                      m_user;

    public UserEditFormViewModel(
        IUsersService usersService,
        IPopupNotificationService popupNotificationService,
        User user
    )
    {
        m_usersService = usersService;
        m_popupNotificationService = popupNotificationService;
        m_user = user;

        Name = user.Name;
        Email = user.Email;
        TelegramTag = user.TelegramTag;
        
        SaveUserCommand = InitializeSaveUserCommand();
    }

    [Reactive]
    public string Name { get; set; }

    [Reactive]
    public string Email { get; set; }
    
    [Reactive]
    public string? TelegramTag { get; set; }

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
    
    public void Reset()
    {
        Email = m_user.Email;
        Name = m_user.Name;
        TelegramTag = m_user.TelegramTag;
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