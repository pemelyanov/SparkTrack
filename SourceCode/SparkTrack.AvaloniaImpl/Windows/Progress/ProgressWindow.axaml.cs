namespace SparkTrack.AvaloniaImpl.Windows.Progress;

using FluentAvalonia.UI.Controls;

public partial class ProgressWindow : ReactiveContentDialog<ProgressViewModel>
{
    public ProgressWindow()
    {
        InitializeComponent();
    }

    protected override void OnCloseButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }
}