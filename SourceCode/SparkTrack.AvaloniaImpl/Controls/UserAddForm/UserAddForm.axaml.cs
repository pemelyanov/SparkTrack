namespace SparkTrack.AvaloniaImpl.Controls.UserAddForm;

using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using Windows;

public partial class UserAddForm : ReactiveContentDialog<UserAddFormViewModel>
{
    public UserAddForm()
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