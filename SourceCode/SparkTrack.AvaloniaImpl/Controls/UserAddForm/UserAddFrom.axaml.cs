namespace SparkTrack.AvaloniaImpl.Controls.UserAddForm;

using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using Windows;

public partial class UserAddFrom : ReactiveContentDialog<UserAddFormViewModel>
{
    public UserAddFrom()
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

    private void CopyPassword_OnClick(object? sender, RoutedEventArgs e)
    {
        if(ViewModel?.GeneratedPassword is not {} generatedPassword) return;

        TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(generatedPassword);
        
        ViewModel.NotifyPasswordCopied();
    }
}