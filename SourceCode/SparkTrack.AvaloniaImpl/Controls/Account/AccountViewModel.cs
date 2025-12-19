namespace SparkTrack.AvaloniaImpl.Controls.Account;

using Core.Client.Services.Authorization;
using Core.Shared.Data.Entities;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using Pages.Authorization;
using Reactive;
using ReactiveUI;

public class AccountViewModel(
    IAuthorizationService authorizationService,
    Lazy<IScreen> screen,
    Func<AuthorizationPageViewModel> authorizationPageFactory
) : ViewModelBase
{
    public IBehaviorObservable<User?> CurrentUser => authorizationService.CurrentUser;

    public async Task LogOutAsync()
    {
        await authorizationService.LogOutAsync();
        screen.Value.Router.PopToOnUIThread(authorizationPageFactory.Invoke());
    }
}