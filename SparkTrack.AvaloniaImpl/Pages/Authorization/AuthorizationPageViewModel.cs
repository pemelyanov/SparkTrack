namespace SparkTrack.AvaloniaImpl.Pages.Authorization;

using Core.Client.Services.Authorization;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using Features;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class AuthorizationPageViewModel(Lazy<IScreen> screen, IAuthorizationService authorizationService, Func<FeaturesPageViewModel> featuresPageViewModelFactory) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment => "auth";

    public IScreen HostScreen => screen.Value;

    [Reactive]
    public string Login { get; set; } = string.Empty;

    [Reactive]
    public string Password { get; set; } = string.Empty;

    public async Task LogInAsync()
    {
        if(string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password)) return;
        
        if(!await authorizationService.LogInAsync(Login, Password)) return;
        
        HostScreen.Router.NavigateOnUIThread(featuresPageViewModelFactory());
    }
}