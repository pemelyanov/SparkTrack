namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

public partial class FeaturePage : ReactiveUserControl<FeaturePageViewModel>
{
    public FeaturePage()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if(sender is not ScrollViewer scrollViewer) return;

        scrollViewer.Offset = new Vector(scrollViewer.Offset.X + e.Delta.Y * 10, 0);
    }
}