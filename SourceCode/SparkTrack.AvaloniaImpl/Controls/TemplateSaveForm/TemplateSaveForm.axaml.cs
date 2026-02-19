using FluentAvalonia.UI.Controls;

namespace SparkTrack.AvaloniaImpl.Controls.TemplateSaveForm;

using Windows;

public partial class TemplateSaveForm : ReactiveContentDialog<TemplateSaveFormViewModel>
{
    public TemplateSaveForm()
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