namespace SparkTrack.AvaloniaImpl.Controls.ChangePasswordForm;

using Core.Client.Enums;
using Core.Client.Services.Authorization;
using Core.Client.Services.PopupNotification;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

public class ChangePasswordFormViewModel : DialogViewModelBase
{
    public ChangePasswordFormViewModel(
        IAuthorizationService authorizationService,
        IPopupNotificationService popupNotificationService
    )
    {
        var canChangePassword = this.WhenAnyValue(it => it.CurrentPassword)
            .CombineLatest(
                this.WhenAnyValue(it => it.NewPassword),
                this.WhenAnyValue(it => it.NewPasswordConfirmation),
                (oldPassword, newPassword, newPasswordConfirmation) =>
                {
                    if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword)
                        || string.IsNullOrWhiteSpace(newPasswordConfirmation)) return false;

                    return newPassword == newPasswordConfirmation;
                }
            );

        ChangePasswordCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                var isSuccess = await authorizationService.ChangePasswordAsync(CurrentPassword, NewPassword);

                if (isSuccess)
                {
                    popupNotificationService.Show(ENotificationType.Success, "Пароль успешно изменен", "Смена пароля");
                    Close(true);
                }
                else
                {
                    popupNotificationService.Show(
                        ENotificationType.Error,
                        "Проверьте текущий пароль",
                        "Ошибка смены пароля"
                    );
                }
            },
            canChangePassword
        );

        CloseCommand = ReactiveCommand.Create(() => Close(false), ChangePasswordCommand.IsExecuting.Select(it => !it));
    }

    [Reactive]
    public string CurrentPassword { get; set; } = string.Empty;

    [Reactive]
    public string NewPassword { get; set; } = string.Empty;

    [Reactive]
    public string NewPasswordConfirmation { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; }

    public ICommand CloseCommand { get; }
}