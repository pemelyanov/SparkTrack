namespace SparkTrack.AvaloniaImpl.Pages.Users;

using Core.Shared.Data.Entities;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels;

public class UsersPageViewModel(Lazy<IScreen> hostScreen) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment => "users";

    public IScreen HostScreen => hostScreen.Value;
    
    [Reactive]
    public bool? CurrentPageSelectionState { get; set; }

    [Reactive]
    public IReadOnlyList<SelectableViewModel<User>> CurrentPageData { get; private set; } = [];
}