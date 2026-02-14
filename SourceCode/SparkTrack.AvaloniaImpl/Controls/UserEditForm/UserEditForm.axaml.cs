namespace SparkTrack.AvaloniaImpl.Controls.UserEditForm;

using Windows;
using FluentAvalonia.UI.Controls;

public partial class UserEditForm : ReactiveContentDialog<UserEditFormViewModel>
{
    public UserEditForm()
    {
        InitializeComponent();
    }
    
    protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }

    protected override void OnSecondaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }

}