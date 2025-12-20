namespace SparkTrack.AvaloniaImpl.Controls.Account;

using Core.Client.Services.Authorization;
using Core.Shared.Data.Entities;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using NLog;
using Pages.Authorization;
using Reactive;
using ReactiveUI;

public class AccountViewModel(
    IAuthorizationService authorizationService,
    Lazy<IScreen> screen,
    Func<AuthorizationPageViewModel> authorizationPageFactory
) : ViewModelBase
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    public IBehaviorObservable<User?> CurrentUser => authorizationService.CurrentUser;

    public async Task LogOutAsync()
    {
        try
        {
            await authorizationService.LogOutAsync();
        }
        catch (Exception e)
        {
            s_logger.Warn(e);
        }

        screen.Value.Router.PopToOnUIThread(authorizationPageFactory.Invoke());
    }
}