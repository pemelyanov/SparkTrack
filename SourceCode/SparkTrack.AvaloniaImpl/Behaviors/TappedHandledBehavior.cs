using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace SparkTrack.AvaloniaImpl.Behaviors;

public class TappedHandledBehavior : StyledElementBehavior<Control>
{
    protected override void OnAttachedToLogicalTree()
    {
        base.OnAttachedToLogicalTree();

        if (AssociatedObject is null) return;
        
        AssociatedObject.Tapped += AssociatedObject_OnTapped;
    }

    protected override void OnDetachedFromLogicalTree()
    {
        base.OnDetachedFromLogicalTree();
        
        if(AssociatedObject is null) return;

        AssociatedObject.Tapped -= AssociatedObject_OnTapped;
    }

    private void AssociatedObject_OnTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
    }
}