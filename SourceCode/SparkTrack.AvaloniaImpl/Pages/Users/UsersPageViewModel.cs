namespace SparkTrack.AvaloniaImpl.Pages.Users;

using Controls.UserEditForm;
using Core.Shared.Data.Entities;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels;

public class UsersPageViewModel(Lazy<IScreen> hostScreen, UserEditFormViewModel userEditFormViewModel)
    : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment => "users";

    public IScreen HostScreen => hostScreen.Value;

    [Reactive]
    public bool? CurrentPageSelectionState { get; set; }

    [Reactive]
    public IReadOnlyList<SelectableViewModel<User>> CurrentPageData { get; private set; } = [];

    public UserEditFormViewModel UserEditFormViewModel { get; } = userEditFormViewModel;
}