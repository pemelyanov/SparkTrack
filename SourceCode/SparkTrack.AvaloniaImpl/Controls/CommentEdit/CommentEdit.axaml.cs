namespace SparkTrack.AvaloniaImpl.Controls.CommentEdit;

using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Extensions;

public partial class CommentEdit : ReactiveUserControl<CommentEditViewModel>
{
    public CommentEdit()
    {
        InitializeComponent();
    }

    private async void TextBox_OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        await this.HandleImagePastingFromClipboard((data, extension) => ViewModel?.OnImagePaste(data, extension));
    }
}