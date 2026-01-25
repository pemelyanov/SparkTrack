namespace SparkTrack.AvaloniaImpl.Controls.BonusForm;

using Windows;
using FluentAvalonia.UI.Controls;

public partial class BonusForm : ReactiveContentDialog<BonusFormViewModel>
{
    public BonusForm()
    {
        InitializeComponent();
    }

    protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }
}