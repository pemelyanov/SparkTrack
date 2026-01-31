namespace SparkTrack.AvaloniaImpl.Controls.BonusForm;

using System.Reactive.Linq;
using System.Windows.Input;
using Core.Shared.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using UsersFilter;
using ViewModels;

public class BonusFormViewModel : DialogViewModelBase
{
    public BonusFormViewModel(UserFilterViewModel userFilterViewModel)
    {
        UserFilterViewModel = userFilterViewModel;
        userFilterViewModel.UserRole = ERole.Employee;
        userFilterViewModel.ShowLabel = false;

        AcceptBonusCommand = ReactiveCommand.Create(
            () => Close(true),
            UserFilterViewModel.WhenAnyValue(it => it.SelectedUser)
                .CombineLatest(
                    this.WhenAnyValue(it => it.Payment),
                    (selectedUser, payment) => selectedUser is not null && payment > 0
                )
        );
    }

    [Reactive]
    public float Payment { get; set; }

    [Reactive]
    public string? Comment { get; set; }

    public UserFilterViewModel UserFilterViewModel { get; }

    public ICommand AcceptBonusCommand { get; }
}