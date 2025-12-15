namespace SparkTrack.AvaloniaImpl.Behaviors;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

public abstract class NumberBoxBehaviorBase : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
    
        AssociatedObject!.AddHandler(InputElement.TextInputEvent, OnInput, RoutingStrategies.Tunnel);
        AssociatedObject!.PastingFromClipboard += OnPastingFromClipboard;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
    
        AssociatedObject!.RemoveHandler(InputElement.TextInputEvent, OnInput);
        AssociatedObject!.PastingFromClipboard -= OnPastingFromClipboard;
    }

    private async void OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        if(sender is not TextBox textBox) return;
        
        var text = await TopLevel.GetTopLevel(AssociatedObject)?.Clipboard?.GetTextAsync()!;

        e.Handled = !IsNumber(text) || !CanInput(textBox.Text, text);

        if (textBox.Text == "0") textBox.Text = string.Empty;
    }

    private void OnInput(object? sender, TextInputEventArgs e)
    {
        if(sender is not TextBox textBox) return;
        
        if (e.Text != null) e.Handled = !IsNumber(e.Text) || !CanInput(textBox.Text, e.Text);
        
        if (textBox.Text == "0") textBox.Text = string.Empty;
    }

    protected abstract bool CanInput(string? source, string? added);

    protected abstract bool IsNumber(string? source);
}