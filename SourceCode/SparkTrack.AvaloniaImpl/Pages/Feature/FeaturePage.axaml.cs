namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

public partial class FeaturePage : ReactiveUserControl<FeaturePageViewModel>
{
    private const string DragOverClass = ":drag-over";
    
    public FeaturePage()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if(sender is not ScrollViewer scrollViewer) return;

        scrollViewer.Offset = new Vector(scrollViewer.Offset.X + e.Delta.Y * 10, 0);
    }

    private void FilesPanel_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if(sender is not Control panel) return;
        
        panel.AddHandler(DragDrop.DragEnterEvent, FilesPanel_OnDragEnter);
        panel.AddHandler(DragDrop.DragLeaveEvent, FilesPanel_OnDragLeave);
        panel.AddHandler(DragDrop.DropEvent, FilesPanel_OnDrop);
    }
    
    private void FilesPanel_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if(sender is not Control panel) return;
        
        panel.RemoveHandler(DragDrop.DragEnterEvent, FilesPanel_OnDragEnter);
        panel.RemoveHandler(DragDrop.DragLeaveEvent, FilesPanel_OnDragLeave);
        panel.RemoveHandler(DragDrop.DropEvent, FilesPanel_OnDrop);
    }

    private void FilesPanel_OnDrop(object? sender, DragEventArgs e)
    {
        FilesPanel_OnDragLeave(sender, e);
    }

    private void FilesPanel_OnDragEnter(object? sender, DragEventArgs e)
    {
        if(sender is not Control panel) return;
        
        ((IPseudoClasses)panel.Classes).Add(DragOverClass);
    }
    
    private void FilesPanel_OnDragLeave(object? sender, DragEventArgs e)
    {
        if(sender is not Control panel) return;
        
        ((IPseudoClasses)panel.Classes).Remove(DragOverClass);
    }
}