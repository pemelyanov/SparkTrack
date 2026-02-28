namespace SparkTrack.AvaloniaImpl.Windows.UserSelection;

using FluentAvalonia.UI.Controls;

public partial class UserSelectionDialog : ReactiveContentDialog<UserSelectionViewModel>
{
    public UserSelectionDialog()
    {
        InitializeComponent();
    }

    protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }
}