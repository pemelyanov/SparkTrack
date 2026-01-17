namespace SparkTrack.AvaloniaImpl.Controls.ChangePasswordForm;

using FluentAvalonia.UI.Controls;
using Windows;

public partial class ChangePasswordForm : ReactiveContentDialog<ChangePasswordFormViewModel>
{
    public ChangePasswordForm()
    {
        InitializeComponent();
    }

    protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }
}