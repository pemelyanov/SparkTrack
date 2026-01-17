namespace SparkTrack.AvaloniaImpl.Controls.Account;

using ChangePasswordForm;
using Core.Client.Services.Authorization;
using Core.Shared.Data.Entities;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using NLog;
using Pages.Authorization;
using Reactive;
using ReactiveUI;
using Services.DialogHost;

public class AccountViewModel(
    IAuthorizationService authorizationService,
    Lazy<IScreen> screen,
    Func<AuthorizationPageViewModel> authorizationPageFactory,
    Func<ChangePasswordFormViewModel> changePasswordFormViewModelFactory,
    IDialogHost dialogHost
) : ViewModelBase
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    public IBehaviorObservable<User?> CurrentUser => authorizationService.CurrentUser;

    public async Task LogOutAsync()
    {
        try
        {
            s_logger.Info("LogOut executed");
            
            if(!await dialogHost.ConfirmAsync("Вы уверены что хотите сменить пользователя?", "Выход")) return;
            
            await authorizationService.LogOutAsync();
        }
        catch (Exception e)
        {
            s_logger.Warn(e);
        }

        screen.Value.Router.PopToOnUIThread(authorizationPageFactory.Invoke());
    }

    public async Task ChangePasswordAsync()
    {
        var viewModel = changePasswordFormViewModelFactory.Invoke();

        await dialogHost.ShowAsync(viewModel);
    }
}