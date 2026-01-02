namespace SparkTrack.AvaloniaImpl.Controls.ProjectEditForm;

using FluentAvalonia.UI.Controls;
using Windows;

public partial class ProjectEditForm : ReactiveContentDialog<ProjectEditFormViewModel>
{
    public ProjectEditForm()
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