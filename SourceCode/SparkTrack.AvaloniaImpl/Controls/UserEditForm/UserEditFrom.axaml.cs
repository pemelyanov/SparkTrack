namespace SparkTrack.AvaloniaImpl.Controls.UserEditForm;

using Windows;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

    protected override void OnSecondaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if(ViewModel?.GeneratedPassword is not {} generatedPassword) return;

        TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(generatedPassword);
    }
}