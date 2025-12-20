namespace SparkTrack.AvaloniaImpl.Controls.UserEditForm;

using Windows;
using FluentAvalonia.UI.Controls;

public partial class UserEditFrom : ReactiveContentDialog<UserEditFormViewModel>
{
    public UserEditFrom()
    {
        InitializeComponent();
    }

    protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }
}