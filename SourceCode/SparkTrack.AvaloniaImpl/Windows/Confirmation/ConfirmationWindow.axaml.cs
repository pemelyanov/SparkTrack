namespace SparkTrack.AvaloniaImpl.Windows.Confirmation;

using FluentAvalonia.UI.Controls;

public partial class ConfirmationWindow : ReactiveContentDialog<ConfirmationViewModel>
{
    public ConfirmationWindow()
    {
        InitializeComponent();
    }

    protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }

    protected override void OnCloseButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }
}